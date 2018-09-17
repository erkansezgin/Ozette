﻿using OzetteLibrary.Files;

namespace OzetteLibrary.CommandLine
{
    /// <summary>
    /// A set of Ozette Azure configuration arguments.
    /// </summary>
    public class AddSourceArguments : Arguments
    {
        /// <summary>
        /// The Azure cloud storage account name.
        /// </summary>
        public string FolderPath { get; set; }

        /// <summary>
        /// The Azure cloud storage account access token.
        /// </summary>
        public FileBackupPriority Priority { get; set; }

        /// <summary>
        /// The Azure cloud storage account access token.
        /// </summary>
        public int Revisions { get; set; }

        /// <summary>
        /// The Azure cloud storage account access token.
        /// </summary>
        public string Matchfilter { get; set; }
    }
}
