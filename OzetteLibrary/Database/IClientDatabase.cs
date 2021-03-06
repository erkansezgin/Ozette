﻿using OzetteLibrary.Files;
using OzetteLibrary.Folders;
using OzetteLibrary.Providers;
using OzetteLibrary.Secrets;
using OzetteLibrary.ServiceCore;
using System;

namespace OzetteLibrary.Database
{
    /// <summary>
    /// A generic database interface for the client database.
    /// </summary>
    public interface IClientDatabase : IDatabase
    {
        /// <summary>
        /// Saves an application setting to the database.
        /// </summary>
        /// <param name="OptionName">Option name</param>
        /// <param name="OptionValue">Option value</param>
        void SetApplicationOption(string OptionName, string OptionValue);

        /// <summary>
        /// Retrieves an application setting value from the database.
        /// </summary>
        /// <remarks>
        /// Returns null if the setting is not found.
        /// </remarks>
        /// <param name="OptionName">Option name</param>
        /// <returns>The setting value.</returns>
        string GetApplicationOption(string OptionName);

        /// <summary>
        /// Removes an application setting value from the database.
        /// </summary>
        /// <param name="OptionName">Option name</param>
        void RemoveApplicationOption(string OptionName);

        /// <summary>
        /// Commits the providers collection to the database.
        /// </summary>
        /// <param name="Providers">A collection of providers.</param>
        void SetProviders(ProvidersCollection Providers);

        /// <summary>
        /// Returns all of the providers defined in the database.
        /// </summary>
        /// <returns>A collection of providers.</returns>
        ProvidersCollection GetProvidersList();

        /// <summary>
        /// Commits the net credentials collection to the database.
        /// </summary>
        /// <param name="Credentials">A collection of net credentials.</param>
        void SetNetCredentialsList(NetCredentialsCollection Credentials);

        /// <summary>
        /// Returns all of the net credentials defined in the database.
        /// </summary>
        /// <returns>A collection of net credentials.</returns>
        NetCredentialsCollection GetNetCredentialsList();

        /// <summary>
        /// Checks the index for a file matching the provided name, path, filesize, and lastmodified date.
        /// </summary>
        /// <param name="FullFilePath">Full file path (file name and path)</param>
        /// <param name="FileSizeBytes">File size in bytes</param>
        /// <param name="FileLastModified">File last modified timestamp</param>
        /// <returns></returns>
        BackupFileLookup GetBackupFile(string FullFilePath, long FileSizeBytes, DateTime FileLastModified);

        /// <summary>
        /// Returns all of the client files in the database.
        /// </summary>
        /// <returns><c>BackupFile</c></returns>
        BackupFiles GetAllBackupFiles();

        /// <summary>
        /// Returns the directory map item for the specified local directory.
        /// </summary>
        /// <remarks>
        /// A new directory map item will be created if none currently exists for the specified folder.
        /// </remarks>
        /// <param name="DirectoryPath">Local directory path. Ex: 'C:\bin\programs'</param>
        /// <returns><c>DirectoryMapItem</c></returns>
        DirectoryMapItem GetDirectoryMapItem(string DirectoryPath);

        /// <summary>
        /// Returns all source locations defined in the database.
        /// </summary>
        /// <returns><c>SourceLocations</c></returns>
        SourceLocations GetAllSourceLocations();

        /// <summary>
        /// Sets a new source locations collection in the database (this will wipe out existing sources).
        /// </summary>
        /// <param name="Locations"><c>SourceLocations</c></param>
        void SetSourceLocations(SourceLocations Locations);

        /// <summary>
        /// Updates a single source location with the specified source.
        /// </summary>
        /// <param name="Location"><c>SourceLocation</c></param>
        void UpdateSourceLocation(SourceLocation Location);

        /// <summary>
        /// Adds a new client file to the database.
        /// </summary>
        /// <param name="File"><c>BackupFile</c></param>
        void AddBackupFile(BackupFile File);

        /// <summary>
        /// Updates an existing client file in the database.
        /// </summary>
        /// <param name="File"><c>BackupFile</c></param>
        void UpdateBackupFile(BackupFile File);

        /// <summary>
        /// Gets the next file that needs to be backed up.
        /// </summary>
        /// <remarks>
        /// If no files need to be backed up, return null.
        /// </remarks>
        /// <returns><c>BackupFile</c></returns>
        BackupFile GetNextFileToBackup();

        /// <summary>
        /// Calculates and returns the overall backup progress.
        /// </summary>
        /// <returns></returns>
        BackupProgress GetBackupProgress();
    }
}
