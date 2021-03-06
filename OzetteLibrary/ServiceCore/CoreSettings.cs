﻿using OzetteLibrary.Constants;
using OzetteLibrary.Exceptions;
using System;

namespace OzetteLibrary.ServiceCore
{
    /// <summary>
    /// Contains functionality for retrieving core settings.
    /// </summary>
    public static class CoreSettings
    {
        /// <summary>
        /// Returns the database connection string.
        /// </summary>
        public static string DatabaseConnectionString
        {
            get
            {
                return GetCoreSetting(BootstrapSettingNames.DatabaseConnectionString);
            }
            set
            {
                SetCoreSetting(BootstrapSettingNames.DatabaseConnectionString, value);
            }
        }

        /// <summary>
        /// Returns the event log name/source.
        /// </summary>
        public static string EventlogName
        {
            get
            {
                return GetCoreSetting(BootstrapSettingNames.EventlogName);
            }
            set
            {
                SetCoreSetting(BootstrapSettingNames.EventlogName, value);
            }
        }

        /// <summary>
        /// Returns the log files directory.
        /// </summary>
        public static string InstallationDirectory
        {
            get
            {
                return GetCoreSetting(BootstrapSettingNames.InstallationDirectory);
            }
            set
            {
                SetCoreSetting(BootstrapSettingNames.InstallationDirectory, value);
            }
        }

        /// <summary>
        /// Returns the log files directory.
        /// </summary>
        public static string LogFilesDirectory
        {
            get
            {
                return GetCoreSetting(BootstrapSettingNames.LogFilesDirectory);
            }
            set
            {
                SetCoreSetting(BootstrapSettingNames.LogFilesDirectory, value);
            }
        }

        /// <summary>
        /// Returns the encryption IV value.
        /// </summary>
        public static string ProtectionIv
        {
            get
            {
                return GetCoreSetting(BootstrapSettingNames.ProtectionIV);
            }
            set
            {
                SetCoreSetting(BootstrapSettingNames.ProtectionIV, value);
            }
        }

        /// <summary>
        /// Gets a core setting value.
        /// </summary>
        /// <param name="name">Name of the setting.</param>
        /// <returns>The setting value.</returns>
        private static string GetCoreSetting(string name)
        {
            var value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ApplicationCoreSettingMissingException(name);
            }

            return value;
        }

        /// <summary>
        /// Sets a core setting value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private static void SetCoreSetting(string name, string value)
        {
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Machine);
        }
    }
}
