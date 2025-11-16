// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Data;
using Properties;
using Sdk.Diagnostics.Logging.Core;
using Sdk.Plugin;
using Sdk.Plugin.Objects;

/// <summary>
/// Manages database operations for the plugin, including schema creation, cleanup, and upgrades.
/// Implements DatabaseManager for core functionality and IDisposable for resource management.
/// </summary>
public class SampleDatabaseManager : DatabaseManager, IDisposable
{
    private const string s_logsCleanupThreshold = "LogThreshold";

    private readonly Logger m_logger;

    public SampleDatabaseManager()
    {
        m_logger = Logger.CreateInstanceLogger(this);
    }

    /// <summary>
    /// Gets the current database configuration.
    /// </summary>
    public DatabaseConfiguration Configuration { get; private set; }

    /// <summary>
    /// Gets the current state of the database.
    /// </summary>
    public DatabaseState State { get; private set; }

    /// <summary>
    /// Releases resources used by the SampleDatabaseManager.
    /// </summary>
    public void Dispose()
    {
        m_logger.Dispose();
    }

    /// <summary>
    /// Returns the SQL script for creating tables, stored procedures, and functions.
    /// </summary>
    /// <param name="databaseName">The name of the database.</param>
    /// <returns>A string containing the SQL creation script.</returns>
    public override string GetSpecificCreationScript(string databaseName)
    {
        return Resources.CreationScript;
    }

    /// <summary>
    /// Executes the database cleanup based on the specified threshold.
    /// </summary>
    /// <param name="name">The name of the cleanup threshold.</param>
    /// <param name="retentionPeriod">The retention period in days.</param>
    public override void DatabaseCleanup(string name, int retentionPeriod)
    {
        if (name == s_logsCleanupThreshold)
        {
            DeleteOldLogs(retentionPeriod);
        }
    }

    /// <summary>
    /// Deletes logs older than the specified number of days.
    /// </summary>
    /// <param name="daysOld">The number of days old logs to delete.</param>
    /// <remarks>
    /// This method uses different database providers depending on the SDK version:
    /// For version 5.11 or earlier: System.Data.SqlClient
    /// For version 5.12 or later: Microsoft.Data.SqlClient
    /// </remarks>
    public void DeleteOldLogs(int daysOld)
    {
        m_logger.TraceDebug($"Deleting logs older than {daysOld} days");

        // When using version 5.11 or earlier, use System.Data.SqlClient.SqlConnection
        //using System.Data.SqlClient.SqlConnection connection = Configuration.CreateSqlConnection();
        //using var command = new System.Data.SqlClient.SqlCommand("DeleteOldLogs", connection);
        //command.CommandType = CommandType.StoredProcedure;
        //command.Parameters.Add(new System.Data.SqlClient.SqlParameter("@DaysOld", daysOld));
        //try
        //{
        //    connection.Open();

        //    int rowsAffected = command.ExecuteNonQuery();
        //    m_logger.TraceDebug($"{rowsAffected} rows were deleted.");
        //}
        //catch (Exception ex)
        //{
        //    m_logger.TraceError(ex, $"An error occurred: {ex.Message}");
        //}

        // When using version 5.12 or later, use Microsoft.Data.SqlClient.SqlConnection
        using Microsoft.Data.SqlClient.SqlConnection connection = Configuration.CreateSqlDatabaseConnection();
        using var command = new Microsoft.Data.SqlClient.SqlCommand("DeleteOldLogs", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@DaysOld", daysOld));
        try
        {
            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();

            m_logger.TraceDebug($"{rowsAffected} rows were deleted.");
        }
        catch (Exception ex)
        {
            m_logger.TraceError(ex, $"An error occurred while deleting logs: {ex.Message}");
        }
    }

    /// <summary>
    /// Inserts a new log entry into the database.
    /// </summary>
    /// <param name="timestamp">The timestamp of the log entry.</param>
    /// <param name="logLevel">The log level.</param>
    /// <param name="message">The log message.</param>
    /// <remarks>
    /// This method uses Microsoft.Data.SqlClient, which is appropriate for SDK version 5.12 or later.
    /// For earlier versions, you would use System.Data.SqlClient instead.
    /// </remarks>
    public void InsertLog(DateTime timestamp, int logLevel, string message)
    {
        // When using version 5.11 or earlier, use System.Data.SqlClient.SqlConnection
        //using System.Data.SqlClient.SqlConnection connection = Configuration.CreateSqlConnection();
        //using var command = new System.Data.SqlClient.SqlCommand("InsertLog", connection);
        //command.CommandType = CommandType.StoredProcedure;

        //command.Parameters.Add(new System.Data.SqlClient.SqlParameter("@Timestamp", timestamp));
        //command.Parameters.Add(new System.Data.SqlClient.SqlParameter("@LogLevel", logLevel));
        //command.Parameters.Add(new System.Data.SqlClient.SqlParameter("@Message", message));
        //try
        //{
        //    connection.Open();
        //    command.ExecuteNonQuery();
        //}
        //catch (Exception ex)
        //{
        //    m_logger.TraceError(ex, $"An error occurred while inserting log: {ex.Message}");
        //}

        // When using version 5.12 or later, use Microsoft.Data.SqlClient.SqlConnection
        using Microsoft.Data.SqlClient.SqlConnection connection = Configuration.CreateSqlDatabaseConnection();
        using var command = new Microsoft.Data.SqlClient.SqlCommand("InsertLog", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@Timestamp", timestamp));
        command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@LogLevel", logLevel));
        command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@Message", message));
        try
        {
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            m_logger.TraceError(ex, $"An error occurred while inserting log: {ex.Message}");
        }
    }

    /// <summary>
    /// Returns the database cleanup thresholds.
    /// </summary>
    /// <returns>An IEnumerable of DatabaseCleanupThreshold objects.</returns>
    public override IEnumerable<DatabaseCleanupThreshold> GetDatabaseCleanupThresholds()
    {
        yield return new DatabaseCleanupThreshold(s_logsCleanupThreshold, "Keep logs");
    }

    /// <summary>
    /// Returns the database upgrade items.
    /// </summary>
    /// <returns>An IEnumerable of DatabaseUpgradeItem objects.</returns>
    public override IEnumerable<DatabaseUpgradeItem> GetDatabaseUpgradeItems()
    {
        // Return the upgrade items for upgrading from version 50001 to 50002
        yield return new DatabaseUpgradeItem(50001, 50002, Resources.UpgradeScript_50002);

        // Return the upgrade items for upgrading from version 50002 to 500003
        yield return new DatabaseUpgradeItem(50002, 50003, Resources.UpgradeScript_50003);
    }

    /// <summary>
    /// Event raised when the database state changes.
    /// </summary>
    public event EventHandler<EventArgs> DatabaseStateChanged;

    /// <summary>
    /// Handles database state changes.
    /// </summary>
    /// <param name="notification">The DatabaseNotification object containing the new state.</param>
    public override void OnDatabaseStateChanged(DatabaseNotification notification)
    {
        State = notification.State; // Update the current state of the database
        DatabaseStateChanged?.Invoke(this, EventArgs.Empty); // Raise the DatabaseStateChanged event
    }

    /// <summary>
    /// Sets the database configuration.
    /// </summary>
    /// <param name="databaseConfiguration">The new DatabaseConfiguration object.</param>
    public override void SetDatabaseInformation(DatabaseConfiguration databaseConfiguration)
    {
        Configuration = databaseConfiguration; // Update the current DatabaseConfiguration
    }
}
