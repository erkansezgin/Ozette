﻿using OzetteLibrary.CommandLine.Arguments;
using OzetteLibrary.Exceptions;
using OzetteLibrary.Files;
using System;
using System.Collections.Generic;

namespace OzetteLibrary.CommandLine
{
    /// <summary>
    /// Contains functionality for parsing Ozette command line arguments.
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// Parses the provided arguments into an <c>Arguments</c> object.
        /// </summary>
        /// <param name="args">The raw arguments from the commandline.</param>
        /// <param name="parsed">An output parameter for the parsed object.</param>
        /// <returns>True if successfully parsed, otherwise false.</returns>
        public bool Parse(string[] args, out ArgumentBase parsed)
        {
            if (args.Length == 0)
            {
                // no args provided.
                parsed = null;
                return false;
            }

            var baseCommand = args[0].ToLower();

            if (baseCommand == "install")
            {
                return ParseInstallArgs(args, out parsed);
            }
            else if (baseCommand == "configure-azure")
            {
                return ParseConfigureAzureArgs(args, out parsed);
            }
            else if (baseCommand == "add-localsource")
            {
                return ParseAddLocalSourceArgs(args, out parsed);
            }
            else if (baseCommand == "add-netsource")
            {
                return ParseAddNetSourceArgs(args, out parsed);
            }
            else if (baseCommand == "add-netcredential")
            {
                return ParseAddNetCredentialArgs(args, out parsed);
            }
            else if (baseCommand == "list-sources")
            {
                // command has no additional arguments
                parsed = new ListSourcesArguments();
                return true;
            }
            else if (baseCommand == "list-netcredentials")
            {
                // command has no additional arguments
                parsed = new ListNetCredentialsArguments();
                return true;
            }
            else if (baseCommand == "list-providers")
            {
                // command has no additional arguments
                parsed = new ListProvidersArguments();
                return true;
            }
            else if (baseCommand == "remove-source")
            {
                return ParseRemoveSourceArgs(args, out parsed);
            }
            else if (baseCommand == "remove-provider")
            {
                return ParseRemoveProviderArgs(args, out parsed);
            }
            else if (baseCommand == "remove-netcredential")
            {
                return ParseRemoveNetCredentialArgs(args, out parsed);
            }
            else if (baseCommand == "show-status")
            {
                // command has no additional arguments
                parsed = new ShowStatusArguments();
                return true;
            }
            else
            {
                // unexpected/no base command provided.
                parsed = null;
                return false;
            }
        }

        /// <summary>
        /// Parses the provided arguments into an <c>InstallationArguments</c> object.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="parsed"></param>
        /// <returns></returns>
        private bool ParseInstallArgs(string[] args, out ArgumentBase parsed)
        {
            // initialize args object with default
            var installArgs = new InstallationArguments();
            var map = ExtractArguments(args);

            if (map.ContainsKey("installdirectory"))
            {
                installArgs.InstallDirectory = map["installdirectory"];
            }
            else
            {
                // apply default
                installArgs.InstallDirectory = Constants.CommandLine.DefaultInstallLocation;
            }

            parsed = installArgs;
            return true;
        }

        /// <summary>
        /// Parses the provided arguments into an <c>ConfigureAzureArguments</c> object.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="parsed"></param>
        /// <returns></returns>
        private bool ParseConfigureAzureArgs(string[] args, out ArgumentBase parsed)
        {
            // initialize args object with default
            var configArgs = new ConfigureAzureArguments();
            var map = ExtractArguments(args);

            if (map.ContainsKey("azurestorageaccountname"))
            {
                configArgs.AzureStorageAccountName = map["azurestorageaccountname"];
            }
            else
            {
                // required argument was not found.
                parsed = null;
                return false;
            }

            if (map.ContainsKey("azurestorageaccounttoken"))
            {
                configArgs.AzureStorageAccountToken = map["azurestorageaccounttoken"];
            }
            else
            {
                // required argument was not found.
                parsed = null;
                return false;
            }

            parsed = configArgs;
            return true;
        }

        /// <summary>
        /// Parses the provided arguments into an <c>AddNetCredentialArguments</c> object.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="parsed"></param>
        /// <returns></returns>
        private bool ParseAddNetCredentialArgs(string[] args, out ArgumentBase parsed)
        {
            // initialize args object with default
            var configArgs = new AddNetCredentialArguments();
            var map = ExtractArguments(args);

            if (map.ContainsKey("credentialname"))
            {
                configArgs.CredentialName = map["credentialname"];
            }
            else
            {
                // required argument was not found.
                parsed = null;
                return false;
            }

            if (map.ContainsKey("username"))
            {
                configArgs.ShareUser = map["username"];
            }
            else
            {
                // required argument was not found.
                parsed = null;
                return false;
            }

            if (map.ContainsKey("password"))
            {
                configArgs.SharePassword = map["password"];
            }
            else
            {
                // required argument was not found.
                parsed = null;
                return false;
            }

            parsed = configArgs;
            return true;
        }

        /// <summary>
        /// Parses the provided arguments into an <c>RemoveNetCredentialArguments</c> object.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="parsed"></param>
        /// <returns></returns>
        private bool ParseRemoveNetCredentialArgs(string[] args, out ArgumentBase parsed)
        {
            // initialize args object with default
            var configArgs = new RemoveNetCredentialArguments();
            var map = ExtractArguments(args);

            if (map.ContainsKey("credentialname"))
            {
                configArgs.CredentialName = map["credentialname"];
            }
            else
            {
                // required argument was not found.
                parsed = null;
                return false;
            }

            parsed = configArgs;
            return true;
        }

        /// <summary>
        /// Parses the provided arguments into an <c>AddLocalSourceArguments</c> object.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="parsed"></param>
        /// <returns></returns>
        private bool ParseAddLocalSourceArgs(string[] args, out ArgumentBase parsed)
        {
            // initialize args object with default
            var sourceArgs = new AddLocalSourceArguments();
            var map = ExtractArguments(args);

            if (map.ContainsKey("folderpath"))
            {
                sourceArgs.FolderPath = map["folderpath"];
            }
            else
            {
                // required argument was not found.
                parsed = null;
                return false;
            }

            if (map.ContainsKey("priority"))
            {
                var priority = map["priority"];
                FileBackupPriority parsedPriority;

                if (Enum.TryParse(priority, true, out parsedPriority))
                {
                    sourceArgs.Priority = parsedPriority;
                }
                else
                {
                    // an optional argument was specified, but was not given a valid value.
                    throw new SourceLocationInvalidFileBackupPriorityException();
                }
            }
            else
            {
                // apply default
                sourceArgs.Priority = Constants.CommandLine.DefaultSourcePriority;
            }

            if (map.ContainsKey("revisions"))
            {
                var revisions = map["revisions"];
                int parsedRevisions;

                if (int.TryParse(revisions, out parsedRevisions))
                {
                    sourceArgs.Revisions = parsedRevisions;
                }
                else
                {
                    // an optional argument was specified, but was not given a valid value.
                    throw new SourceLocationInvalidRevisionCountException();
                }
            }
            else
            {
                // apply default
                sourceArgs.Revisions = Constants.CommandLine.DefaultSourceRevisionCount;
            }

            if (map.ContainsKey("matchfilter"))
            {
                sourceArgs.Matchfilter = map["matchfilter"];
            }
            else
            {
                // apply default
                sourceArgs.Matchfilter = Constants.CommandLine.DefaultSourceMatchFilter;
            }

            parsed = sourceArgs;
            return true;
        }

        /// <summary>
        /// Parses the provided arguments into an <c>AddNetSourceArguments</c> object.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="parsed"></param>
        /// <returns></returns>
        private bool ParseAddNetSourceArgs(string[] args, out ArgumentBase parsed)
        {
            // initialize args object with default
            var sourceArgs = new AddNetSourceArguments();
            var map = ExtractArguments(args);

            if (map.ContainsKey("uncpath"))
            {
                sourceArgs.UncPath = map["uncpath"];
            }
            else
            {
                // required argument was not found.
                parsed = null;
                return false;
            }

            if (map.ContainsKey("credentialname"))
            {
                // optional argument (not all net shares are authenticated)
                sourceArgs.CredentialName = map["credentialname"];
            }

            if (map.ContainsKey("priority"))
            {
                var priority = map["priority"];
                FileBackupPriority parsedPriority;

                if (Enum.TryParse(priority, true, out parsedPriority))
                {
                    sourceArgs.Priority = parsedPriority;
                }
                else
                {
                    // an optional argument was specified, but was not given a valid value.
                    throw new SourceLocationInvalidFileBackupPriorityException();
                }
            }
            else
            {
                // apply default
                sourceArgs.Priority = Constants.CommandLine.DefaultSourcePriority;
            }

            if (map.ContainsKey("revisions"))
            {
                var revisions = map["revisions"];
                int parsedRevisions;

                if (int.TryParse(revisions, out parsedRevisions))
                {
                    sourceArgs.Revisions = parsedRevisions;
                }
                else
                {
                    // an optional argument was specified, but was not given a valid value.
                    throw new SourceLocationInvalidRevisionCountException();
                }
            }
            else
            {
                // apply default
                sourceArgs.Revisions = Constants.CommandLine.DefaultSourceRevisionCount;
            }

            if (map.ContainsKey("matchfilter"))
            {
                sourceArgs.Matchfilter = map["matchfilter"];
            }
            else
            {
                // apply default
                sourceArgs.Matchfilter = Constants.CommandLine.DefaultSourceMatchFilter;
            }

            parsed = sourceArgs;
            return true;
        }

        /// <summary>
        /// Parses the provided arguments into an <c>RemoveSourceArguments</c> object.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="parsed"></param>
        /// <returns></returns>
        private bool ParseRemoveSourceArgs(string[] args, out ArgumentBase parsed)
        {
            // initialize args object with default
            var remSrcArgs = new RemoveSourceArguments();
            var map = ExtractArguments(args);

            if (map.ContainsKey("sourceid"))
            {
                var sourceId = map["sourceid"];
                int parsedId;

                if (int.TryParse(sourceId, out parsedId))
                {
                    remSrcArgs.SourceID = parsedId;
                }
                else
                {
                    // required argument was not valid.
                    parsed = null;
                    return false;
                }
            }
            else
            {
                // required argument was not found.
                parsed = null;
                return false;
            }

            parsed = remSrcArgs;
            return true;
        }

        /// <summary>
        /// Parses the provided arguments into an <c>RemoveProviderArguments</c> object.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="parsed"></param>
        /// <returns></returns>
        private bool ParseRemoveProviderArgs(string[] args, out ArgumentBase parsed)
        {
            // initialize args object with default
            var remSrcArgs = new RemoveProviderArguments();
            var map = ExtractArguments(args);

            if (map.ContainsKey("providerid"))
            {
                var providerId = map["providerid"];
                int parsedId;

                if (int.TryParse(providerId, out parsedId))
                {
                    remSrcArgs.ProviderID = parsedId;
                }
                else
                {
                    // required argument was not valid.
                    parsed = null;
                    return false;
                }
            }
            else
            {
                // required argument was not found.
                parsed = null;
                return false;
            }

            parsed = remSrcArgs;
            return true;
        }

        /// <summary>
        /// Returns a dictionary map of the provided arguments.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private Dictionary<string, string> ExtractArguments(string[] args)
        {
            var map = new Dictionary<string, string>();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    if (i + 1 > args.Length || args[i+1].StartsWith("--"))
                    {
                        // there is no next argumnt
                        // or the next argument is another parameter option
                        // this means no value is provided. assume a switch param.
                        map.Add(args[i].ToLower().Substring(2), "true");
                    }
                    else
                    {
                        map.Add(args[i].ToLower().Substring(2), args[i + 1]);
                    }
                }
            }

            return map;
        }
    }
}
