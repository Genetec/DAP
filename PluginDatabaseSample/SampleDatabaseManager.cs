// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using Properties;
    using Sdk.Diagnostics.Logging.Core;
    using Sdk.Plugin;
    using Sdk.Plugin.Objects;

    public class SampleDatabaseManager : DatabaseManager, IDisposable
    {
        private const string LogsCleanupThreshold = "LogThreshold";

        private readonly Logger m_logger;

        public SampleDatabaseManager()
        {
            m_logger = Logger.CreateInstanceLogger(this);
        }

        public DatabaseConfiguration Configuration { get; private set; }

        public DatabaseState State { get; private set; }

        public void Dispose()
        {
            m_logger.Dispose();
        }

        public override string GetSpecificCreationScript(string databaseName)
        {
            return Resources.CreationScript;
        }

        public override void DatabaseCleanup(string name, int retentionPeriod)
        {
            if (name == LogsCleanupThreshold)
            {
                DeleteOldLogs(retentionPeriod);
            }
        }

        public void DeleteOldLogs(int daysOld)
        {
            m_logger.TraceDebug($"Deleting logs older than {daysOld} days");

            using SqlConnection connection = Configuration.CreateSqlConnection();
            try
            {
                using var command = new SqlCommand("DeleteOldLogs", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@DaysOld", daysOld));

                connection.Open();

                int rowsAffected = command.ExecuteNonQuery();

                m_logger.TraceDebug($"{rowsAffected} rows were deleted.");
            }
            catch (Exception ex)
            {
                m_logger.TraceError(ex, $"An error occurred: {ex.Message}");
            }
        }

        public void InsertLog(DateTime timestamp, int logLevel, string message)
        {
            using SqlConnection connection = Configuration.CreateSqlConnection();
            try
            {
                using var command = new SqlCommand("InsertLog", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@Timestamp", timestamp));
                command.Parameters.Add(new SqlParameter("@LogLevel", logLevel));
                command.Parameters.Add(new SqlParameter("@Message", message));

                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                m_logger.TraceError(ex, $"An error occurred while inserting log: {ex.Message}");
            }
        }

        public override IEnumerable<DatabaseCleanupThreshold> GetDatabaseCleanupThresholds()
        {
            yield return new DatabaseCleanupThreshold(LogsCleanupThreshold, "Keep logs");
        }

        public override IEnumerable<DatabaseUpgradeItem> GetDatabaseUpgradeItems()
        {
            yield return new DatabaseUpgradeItem(50001, 500002, Resources.UpgradeScript_50002);
            yield return new DatabaseUpgradeItem(50002, 500003, Resources.UpgradeScript_50003);
        }

        public event EventHandler<EventArgs> DatabaseStateChanged;

        public override void OnDatabaseStateChanged(DatabaseNotification notification)
        {
            State = notification.State;
            DatabaseStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public override void SetDatabaseInformation(DatabaseConfiguration databaseConfiguration)
        {
            Configuration = databaseConfiguration;
        }
    }
}