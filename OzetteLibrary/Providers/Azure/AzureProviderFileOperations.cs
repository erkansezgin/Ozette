﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using OzetteLibrary.Constants;
using OzetteLibrary.Crypto;
using OzetteLibrary.Files;
using OzetteLibrary.Folders;
using OzetteLibrary.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace OzetteLibrary.Providers.Azure
{
    /// <summary>
    /// Contains file operations for the Azure cloud storage provider.
    /// </summary>
    public class AzureProviderFileOperations : IProviderFileOperations
    {
        /// <summary>
        /// A reference to the logging utility.
        /// </summary>
        private ILogger Logger;

        /// <summary>
        /// A reference to the hashing helper instance.
        /// </summary>
        private Hasher Hasher;

        /// <summary>
        /// A reference to the provider utilities helper instance.
        /// </summary>
        private AzureProviderUtilities ProviderUtilities;

        /// <summary>
        /// A reference to the authenticated Azure Storage account.
        /// </summary>
        private CloudStorageAccount AzureStorage;

        /// <summary>
        /// Constructor that accepts a storage account name and storage account SAS token.
        /// </summary>
        /// <param name="logger">A logging utility instance.</param>
        /// <param name="storageAccountName">The azure storage account name.</param>
        /// <param name="storageAccountSASToken">SAS token for accessing the resource.</param>
        public AzureProviderFileOperations(ILogger logger, string storageAccountName, string storageAccountSASToken)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if (string.IsNullOrWhiteSpace(storageAccountName))
            {
                throw new ArgumentException(nameof(storageAccountName) + " must be provided.");
            }
            if (string.IsNullOrWhiteSpace(storageAccountSASToken))
            {
                throw new ArgumentException(nameof(storageAccountSASToken) + " must be provided.");
            }

            Logger = logger;
            Hasher = new Hasher(logger);
            ProviderUtilities = new AzureProviderUtilities();

            var storageCredentials = new StorageCredentials(storageAccountName, storageAccountSASToken);
            AzureStorage = new CloudStorageAccount(storageCredentials, true);
        }

        /// <summary>
        /// Returns the status of a file as it exists (or doesn't) in Azure cloud storage.
        /// </summary>
        /// <param name="file"><c>BackupFile</c></param>
        /// <param name="directory"><c>DirectoryMapItem</c></param>
        /// <returns><c>ProviderFileStatus</c></returns>
        public async Task<ProviderFileStatus> GetFileStatusAsync(BackupFile file, DirectoryMapItem directory)
        {
            Logger.WriteTraceMessage("Checking the Azure provider status.");
            
            // calculate my uri

            var sasBlobUri = ProviderUtilities.GetFileUri(AzureStorage.Credentials.AccountName, directory.GetRemoteContainerName(ProviderTypes.Azure), file.GetRemoteFileName(ProviderTypes.Azure));

            // the default state for a freshly initialized file status object is unsynced.
            // if the blob doesn't exist, the file is unsynced.

            CloudBlockBlob blob = new CloudBlockBlob(new Uri(sasBlobUri), AzureStorage.Credentials);
            var fileStatus = new ProviderFileStatus(ProviderTypes.Azure);

            // does the file exist at the specified uri?

            if (await blob.ExistsAsync().ConfigureAwait(false))
            {
                // -- query metadata
                // -- determine state from metadata
                
                await blob.FetchAttributesAsync().ConfigureAwait(false);

                fileStatus.ApplyMetadataToState(blob.Metadata);
            }
            
            Logger.WriteTraceMessage("File sync status in Azure: " + fileStatus.SyncStatus);
            return fileStatus;
        }

        /// <summary>
        /// Uploads a single block of a file to Azure cloud storage.
        /// </summary>
        /// <remarks>
        /// If this file is the only (or final) block in the file, the file should be committed and/or the transaction finalized.
        /// </remarks>
        /// <param name="file"><c>BackupFile</c></param>
        /// <param name="directory"><c>DirectoryMapItem</c></param>
        /// <param name="data">A byte array stream of file contents/data.</param>
        /// <param name="currentBlockIndex">The block number associated with the specified data.</param>
        /// <param name="totalBlocks">The total number of blocks that this file is made of.</param>
        public async Task UploadFileBlockAsync(BackupFile file, DirectoryMapItem directory, byte[] data, int currentBlockIndex, int totalBlocks)
        {
            var containerName = directory.GetRemoteContainerName(ProviderTypes.Azure);
            var containerUri = ProviderUtilities.GetContainerUri(AzureStorage.Credentials.AccountName, containerName);
            var currentBlockNumber = currentBlockIndex + 1;

            CloudBlobContainer container = new CloudBlobContainer(new Uri(containerUri), AzureStorage.Credentials);

            if (!await container.ExistsAsync().ConfigureAwait(false))
            {
                Logger.WriteTraceMessage(string.Format("Azure container [{0}] does not exist. Creating it now.", containerName));
                await container.CreateAsync().ConfigureAwait(false);
            }

            Logger.WriteTraceMessage(string.Format("Uploading file block ({0} of {1}) to Azure storage.", currentBlockNumber, totalBlocks));

            // calculate my uri

            var sasBlobUri = ProviderUtilities.GetFileUri(AzureStorage.Credentials.AccountName, containerName, file.GetRemoteFileName(ProviderTypes.Azure));

            // hash the block. Azure has an integrity check on the server side if we supply the expected md5 hash.

            string blockMd5Hash = Hasher.ConvertHashByteArrayToBase64EncodedString(
                Hasher.HashFileBlockFromByteArray(HashAlgorithmName.MD5, data)
            );

            // get a current reference to the blob and it's attributes.
            // upload the block and expected hash.

            CloudBlockBlob blob = new CloudBlockBlob(new Uri(sasBlobUri), AzureStorage.Credentials);

            using (var stream = new MemoryStream(data))
            {
                var encodedBlockIdString = ProviderUtilities.GenerateBlockIdentifierBase64String(file.FileID, currentBlockIndex);
                await blob.PutBlockAsync(encodedBlockIdString, stream, blockMd5Hash).ConfigureAwait(false);
            }

            // after the block has been uploaded it is in an uncommitted state.
            // commit this block (plus any previously committed blocks).

            var blockListToCommit = ProviderUtilities.GenerateListOfBlocksToCommit(file.FileID, currentBlockIndex);
            await blob.PutBlockListAsync(blockListToCommit).ConfigureAwait(false);

            // update metadata/status

            blob.Metadata[ProviderMetadata.ProviderSyncStatusKeyName] = 
                (currentBlockNumber == totalBlocks ? 
                    FileStatus.Synced.ToString() :
                    FileStatus.InProgress.ToString());

            blob.Metadata[ProviderMetadata.ProviderLastCompletedFileBlockIndexKeyName] = currentBlockIndex.ToString();
            blob.Metadata[ProviderMetadata.FullSourcePathKeyName] = file.FullSourcePath;
            blob.Metadata[ProviderMetadata.FileHash] = file.FileHashString;
            blob.Metadata[ProviderMetadata.FileHashAlgorithm] = file.HashAlgorithmType;

            // set metadata.
            await blob.SetMetadataAsync().ConfigureAwait(false);

            if (currentBlockNumber == totalBlocks)
            {
                if (!blob.Properties.StandardBlobTier.HasValue
                    || blob.Properties.StandardBlobTier.Value != StandardBlobTier.Archive)
                {
                    // set blob tier access.
                    // we only need to set this one time, at the time the upload is completed.
                    await blob.SetStandardBlobTierAsync(StandardBlobTier.Archive);
                }

                Logger.WriteTraceMessage("File upload completed successfully.");
            }
        }
    }
}
