﻿using System;
using System.Configuration;

namespace OzetteLibrary.ServiceCore
{
    /// <summary>
    /// Contains options for windows services.
    /// </summary>
    public class ServiceOptions
    {
        /// <summary>
        /// The log files directory.
        /// </summary>
        public string LogFilesDirectory { get; set; }

        /// <summary>
        /// The custom windows event log name.
        /// </summary>
        public string EventlogName { get; set; }

        /// <summary>
        /// The database file name.
        /// </summary>
        public string DatabaseConnectionString { get; set; }

        /// <summary>
        /// Base constructor.
        /// </summary>
        public ServiceOptions()
        {
        }

        /// <summary>
        /// Constructor that accepts a settings property collection.
        /// </summary>
        /// <param name="settings"></param>
        public ServiceOptions(SettingsPropertyCollection settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            LogFilesDirectory = (settings[nameof(LogFilesDirectory)] as SettingsProperty)?.DefaultValue.ToString();
            EventlogName = (settings[nameof(EventlogName)] as SettingsProperty)?.DefaultValue.ToString();
            DatabaseConnectionString = (settings[nameof(DatabaseConnectionString)] as SettingsProperty)?.DefaultValue.ToString();
        }
    }
}
