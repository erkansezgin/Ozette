﻿using OzetteLibrary.Client;
using OzetteLibrary.Database.LiteDB;
using OzetteLibrary.Logging;
using OzetteLibrary.Logging.Default;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace OzetteClientAgent
{
    /// <summary>
    /// Contains service functionality.
    /// </summary>
    public partial class OzetteClientAgent : ServiceBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public OzetteClientAgent()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Runs when the service start is triggered.
        /// </summary>
        /// <remarks>
        /// Long running initialization code can confuse the service control manager (thinks it may be a hang).
        /// Instead launch the initialization tasks in a seperate thread so control returns to the SCM immediately.
        /// </remarks>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            Thread t = new Thread(() => CoreStart());
        }

        /// <summary>
        /// A reference to the core service log.
        /// </summary>
        private ILogger CoreLog { get; set; }

        /// <summary>
        /// A reference to the scanning engine instance.
        /// </summary>
        private ScanEngine Scan { get; set; }

        /// <summary>
        /// A reference to the backup engine instance.
        /// </summary>
        private BackupEngine Backup { get; set; }
        
        /// <summary>
        /// Core application start.
        /// </summary>
        private void CoreStart()
        {
            // in the client agent the core loop consists of two pieces.
            // first is the scan engine, and the second is the backup engine.
            // each one lives under it's own long-running thread and class.
            // prepare the database and then start both engines.

            CoreLog = new Logger(OzetteLibrary.Constants.Logging.CoreServiceComponentName);

            CoreLog.Start(
                Properties.Settings.Default.EventlogName, 
                Properties.Settings.Default.EventlogName, 
                Properties.Settings.Default.LogFilesDirectory);

            CoreLog.WriteSystemEvent(
                string.Format("Starting {0} client service.", OzetteLibrary.Constants.Logging.AppName), 
                EventLogEntryType.Information, OzetteLibrary.Constants.EventIDs.StartingService);

            StartScanEngine();
            StartBackupEngine();

            CoreLog.WriteSystemEvent(
                string.Format("Successfully started {0} client service.", OzetteLibrary.Constants.Logging.AppName),
                EventLogEntryType.Information, OzetteLibrary.Constants.EventIDs.StartedService);
        }

        /// <summary>
        /// Runs when the service stop is triggered.
        /// </summary>
        protected override void OnStop()
        {
            if (CoreLog != null)
            {
                CoreLog.WriteSystemEvent(
                    string.Format("Stopping {0} client service.", OzetteLibrary.Constants.Logging.AppName),
                    EventLogEntryType.Information, OzetteLibrary.Constants.EventIDs.StoppingService);
            }

            if (Scan != null)
            {
                Scan.BeginStop();
            }
            if (Backup != null)
            {
                Backup.BeginStop();
            }

            if (CoreLog != null)
            {
                CoreLog.WriteSystemEvent(
                    string.Format("Successfully stopped {0} client service.", OzetteLibrary.Constants.Logging.AppName),
                    EventLogEntryType.Information, OzetteLibrary.Constants.EventIDs.StoppedService);
            }
        }

        /// <summary>
        /// Starts the scanning engine.
        /// </summary>
        private void StartScanEngine()
        {
            // note: each engine can get it's own instance of the LiteDBClientDatabase wrapper.
            // LiteDB is thread safe, but the wrapper is not; so give threads their own DB wrappers.

            var log = new Logger(OzetteLibrary.Constants.Logging.ScanningComponentName);

            log.Start(
                Properties.Settings.Default.EventlogName,
                Properties.Settings.Default.EventlogName,
                Properties.Settings.Default.LogFilesDirectory);

            var db = new LiteDBClientDatabase(Properties.Settings.Default.DatabaseConnectionString, log);
            db.PrepareDatabase();

            Scan = new ScanEngine(db, log);
            Scan.Stopped += Scan_Stopped;
            Scan.BeginStart();

            CoreLog.WriteTraceMessage("Scanning Engine has started.");

            CoreLog.WriteSystemEvent(
                string.Format("Scanning Engine has started."),
                EventLogEntryType.Information, OzetteLibrary.Constants.EventIDs.StartedScanEngine);
        }

        /// <summary>
        /// Callback event for when scanning engine has stopped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scan_Stopped(object sender, OzetteLibrary.Events.EngineStoppedEventArgs e)
        {
            if (e.Reason == OzetteLibrary.Events.EngineStoppedReason.Failed)
            {
                CoreLog.WriteTraceError("Scanning Engine has failed.", e.Exception, CoreLog.GenerateFullContextStackTrace());

                CoreLog.WriteSystemEvent(
                    string.Format("Scanning Engine has failed."),
                    e.Exception,
                    CoreLog.GenerateFullContextStackTrace(),
                    OzetteLibrary.Constants.EventIDs.FailedScanEngine);
            }
            else if (e.Reason == OzetteLibrary.Events.EngineStoppedReason.StopRequested)
            {
                CoreLog.WriteTraceMessage("Scanning Engine has stopped.");

                CoreLog.WriteSystemEvent(
                    string.Format("Scanning Engine has stopped."),
                    EventLogEntryType.Information, OzetteLibrary.Constants.EventIDs.StoppedScanEngine);
            }
            else
            {
                throw new InvalidOperationException("Unexpected EngineStoppedReason: " + e.Reason);
            }
        }

        /// <summary>
        /// Starts the backup engine.
        /// </summary>
        private void StartBackupEngine()
        {
            // note: each engine can get it's own instance of the LiteDBClientDatabase wrapper.
            // LiteDB is thread safe, but the wrapper is not; so give threads their own DB wrappers.

            var log = new Logger(OzetteLibrary.Constants.Logging.BackupComponentName);

            log.Start(
                Properties.Settings.Default.EventlogName,
                Properties.Settings.Default.EventlogName,
                Properties.Settings.Default.LogFilesDirectory);

            var db = new LiteDBClientDatabase(Properties.Settings.Default.DatabaseConnectionString, log);
            db.PrepareDatabase();

            Backup = new BackupEngine(db, log);
            Backup.Stopped += Backup_Stopped;
            Backup.BeginStart();

            CoreLog.WriteTraceMessage("Backup Engine has started.");

            CoreLog.WriteSystemEvent(
                string.Format("Backup Engine has started."),
                EventLogEntryType.Information, OzetteLibrary.Constants.EventIDs.StartedBackupEngine);
        }

        /// <summary>
        /// Callback event for when the backup engine has stopped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Backup_Stopped(object sender, OzetteLibrary.Events.EngineStoppedEventArgs e)
        {
            if (e.Reason == OzetteLibrary.Events.EngineStoppedReason.Failed)
            {
                CoreLog.WriteTraceError("Backup Engine has failed.", e.Exception, CoreLog.GenerateFullContextStackTrace());

                CoreLog.WriteSystemEvent(
                    string.Format("Backup Engine has failed."),
                    e.Exception,
                    CoreLog.GenerateFullContextStackTrace(),
                    OzetteLibrary.Constants.EventIDs.FailedBackupEngine);
            }
            else if (e.Reason == OzetteLibrary.Events.EngineStoppedReason.StopRequested)
            {
                CoreLog.WriteTraceMessage("Backup Engine has stopped.");

                CoreLog.WriteSystemEvent(
                    string.Format("Backup Engine has stopped."),
                    EventLogEntryType.Information, OzetteLibrary.Constants.EventIDs.StoppedBackupEngine);
            }
            else
            {
                throw new InvalidOperationException("Unexpected EngineStoppedReason: " + e.Reason);
            }
        }
    }
}
