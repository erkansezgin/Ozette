﻿using OzetteLibrary.Database.LiteDB;
using OzetteLibrary.Exceptions;
using OzetteLibrary.Folders;
using OzetteLibrary.Logging.Default;
using OzetteLibrary.ServiceCore;
using System;
using System.Diagnostics;
using System.Linq;

namespace OzetteLibrary.CommandLine.Commands
{
    /// <summary>
    /// A command for adding backup sources to the active configuration.
    /// </summary>
    public static class AddSource
    {
        /// <summary>
        /// A logging helper instance.
        /// </summary>
        private static Logger Logger = new Logger(Constants.Logging.CommandLineComponentName);

        /// <summary>
        /// Runs the add-source command.
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns>True if successful, otherwise false.</returns>
        public static bool Run(AddSourceArguments arguments)
        {
            try
            {
                Logger.WriteConsole("--- Starting Ozette Cloud Backup configuration: Add a source");

                Logger.WriteConsole("--- Step 1: Validate the source and save it to the database.");
                ValidateSource(arguments);

                Logger.WriteConsole("--- Source configuration completed successfully.");

                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteConsole("--- Ozette Cloud Backup Azure configuration failed", EventLogEntryType.Error);
                Logger.WriteConsole(ex.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// Validates the provided source is usable.
        /// </summary>
        /// <param name="arguments"></param>
        private static void ValidateSource(AddSourceArguments arguments)
        {
            Logger.WriteConsole("Initializing a database connection.");

            var db = new LiteDBClientDatabase(CoreSettings.DatabaseConnectionString);
            db.PrepareDatabase();

            Logger.WriteConsole("Querying for existing scan sources to check for duplicates.");

            var allSources = db.GetAllSourceLocations();

            if (allSources.Any(x => x.FolderPath.ToLower() == arguments.FolderPath.ToLower() && x.FileMatchFilter == arguments.Matchfilter))
            {
                // there already exists a source with this folder location and match filter.
                throw new SourceLocationException("Unable to add source: the specified folder and match filter combination is already listed as a source.");
            }
            else
            {
                Logger.WriteConsole("No duplicate sources found.");
            }

            var newSource = new SourceLocation();
            newSource.FolderPath = arguments.FolderPath;
            newSource.FileMatchFilter = arguments.Matchfilter;
            newSource.RevisionCount = arguments.Revisions;
            newSource.Priority = arguments.Priority;

            int highestKnownID = 0;
            foreach (var source in allSources)
            {
                if (source.ID > highestKnownID)
                {
                    highestKnownID = source.ID;
                }
            }

            newSource.ID = highestKnownID + 1;

            Logger.WriteConsole("Validating the source is usable.");
            newSource.Validate();
            Logger.WriteConsole("The specified scan source is valid and can be used.");

            Logger.WriteConsole("Saving the source to the database.");
            allSources.Add(newSource);
            db.SetSourceLocations(allSources);
            Logger.WriteConsole(string.Format("Successfully saved source ID {0} to the database.", newSource.ID));
        }
    }
}