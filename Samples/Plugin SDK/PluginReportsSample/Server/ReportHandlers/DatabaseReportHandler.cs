// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Plugin.Objects;
using Genetec.Sdk.Queries;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Base class for the handlers that read their records from a table of the plugin database.
/// Owns everything the handlers have in common: the database availability check, the connection,
/// the TOP clause, the time range condition, the WHERE assembly, the ordering, and the streaming
/// of the reader. A derived handler only declares its table, its columns, its query-specific
/// filters, and how a row maps to a record.
/// </summary>
public abstract class DatabaseReportHandler<TQuery, TRecord> : ReportHandler<TQuery, TRecord> where TQuery : ReportQuery
{
    private readonly SampleDatabaseManager m_databaseManager;

    protected DatabaseReportHandler(IEngine engine, Role role, SampleDatabaseManager databaseManager) : base(engine, role)
    {
        m_databaseManager = databaseManager;
    }

    /// <summary>
    /// Gets the table read by this handler.
    /// </summary>
    protected abstract string TableName { get; }

    /// <summary>
    /// Gets the selected columns, in the order <see cref="MapRecord"/> reads them.
    /// </summary>
    protected abstract string SelectColumns { get; }

    /// <summary>
    /// Gets the column used for the time range condition and the ordering.
    /// Return null when the table is not time-based.
    /// </summary>
    protected virtual string TimestampColumn => "EventTimestamp";

    protected sealed override async IAsyncEnumerable<TRecord> GetRecordsAsync(TQuery query, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using SqlConnection connection = m_databaseManager.Configuration.CreateSqlDatabaseConnection();
        await connection.OpenAsync(cancellationToken);

        using SqlCommand command = await CreateSelectCommand(connection, query);
        using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        // Stream the records instead of loading them all in memory; the ReportHandler
        // base class batches them and sends partial results as they become available.
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return MapRecord(reader);
        }
    }

    // Translates the query filters into a parameterized SQL query
    private async Task<SqlCommand> CreateSelectCommand(SqlConnection connection, TQuery query)
    {
        var command = new SqlCommand { Connection = connection };
        var sql = new StringBuilder("SELECT")
            .Append(SqlFilterBuilder.Top(query.MaximumResultCount))
            .Append(' ').Append(SelectColumns)
            .Append(" FROM ").Append(TableName);

        var conditions = new List<string>();

        // Time range filter: TimeRange.DateTime is the UTC start and TimeRange.TimeSpan is the duration
        if (TimestampColumn is not null && query.TimeRange.IsSet)
        {
            DateTime rangeStart = query.TimeRange.DateTime;
            // Open-ended ranges can carry a duration beyond DateTime.MaxValue.
            DateTime rangeEnd = query.TimeRange.TimeSpan >= DateTime.MaxValue - rangeStart
                ? DateTime.MaxValue
                : rangeStart + query.TimeRange.TimeSpan;
            conditions.Add($"{TimestampColumn} BETWEEN @RangeStart AND @RangeEnd");
            command.Parameters.Add("@RangeStart", SqlDbType.DateTime2).Value = rangeStart;
            command.Parameters.Add("@RangeEnd", SqlDbType.DateTime2).Value = rangeEnd;
        }

        await AddFiltersAsync(conditions, command, query);

        SqlFilterBuilder.AppendConditions(sql, conditions);

        if (TimestampColumn is not null)
        {
            sql.Append(" ORDER BY ").Append(TimestampColumn)
                .Append(query.SortOrder == OrderByType.Descending ? " DESC" : " ASC");
        }

        command.CommandText = sql.ToString();
        return command;
    }

    /// <summary>
    /// Override to translate the query-specific filters into SQL conditions.
    /// </summary>
    protected virtual Task AddFiltersAsync(ICollection<string> conditions, SqlCommand command, TQuery query)
        => Task.CompletedTask;

    /// <summary>
    /// Maps the current row of the reader, whose columns follow <see cref="SelectColumns"/>, to a record.
    /// </summary>
    protected abstract TRecord MapRecord(SqlDataReader reader);
}
