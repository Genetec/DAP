// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.EventsArgs;
using Genetec.Sdk.Plugin.Objects;
using Genetec.Sdk.Queries;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Base class for the handlers that read their records from a table of the plugin database.
/// Owns everything the handlers have in common: the database connection,
/// the TOP clause, the time range condition, the WHERE assembly, the ordering, and the streaming
/// of the reader. A derived handler only declares its table, its columns, its query-specific
/// filters, and how a reader record is added to the result table.
/// </summary>
public abstract class DatabaseReportHandler<TQuery> : IReportHandler where TQuery : ReportQuery
{
    private readonly SampleDatabaseManager m_databaseManager;

    protected DatabaseReportHandler(IEngine engine, Role role, SampleDatabaseManager databaseManager)
    {
        Engine = engine;
        Role = role;
        m_databaseManager = databaseManager;
    }

    protected IEngine Engine { get; }
    protected Role Role { get; }

    /// <summary>
    /// Gets the table read by this handler.
    /// </summary>
    protected abstract string TableName { get; }

    /// <summary>
    /// Gets the selected columns read by <see cref="AddRow"/>.
    /// </summary>
    protected abstract string SelectColumns { get; }

    /// <summary>
    /// Gets the column used for the time range condition and the ordering.
    /// Return null when the table is not time-based.
    /// </summary>
    protected virtual string TimestampColumn => "EventTimestamp";

    public async Task<ReportError> HandleAsync(ReportQueryReceivedEventArgs args, CancellationToken cancellationToken)
    {
        if (args.Query is not TQuery query || !IsQuerySupported(query))
        {
            return ReportError.None;
        }

        using SqlConnection connection = m_databaseManager.Configuration.CreateSqlDatabaseConnection();
        await connection.OpenAsync(cancellationToken);

        using SqlCommand command = await CreateSelectCommand(connection, query);
        using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int totalSent = 0;
        int maximumResultCount = query.MaximumResultCount;
        DataTable table = CreateDataTable(query);

        while (await reader.ReadAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            // The command reads one extra row to detect overflow. Send the rows accumulated so
            // far, but do not add or send the extra row.
            if (maximumResultCount > 0 && totalSent + table.Rows.Count >= maximumResultCount)
            {
                SendQueryResult(args, table);
                return ReportError.TooManyResults;
            }

            AddRow(table, reader);

            if (table.Rows.Count == 100)
            {
                SendQueryResult(args, table);
                totalSent += table.Rows.Count;
                table = CreateDataTable(query);
            }
        }

        SendQueryResult(args, table);
        return ReportError.None;
    }

    protected virtual bool IsQuerySupported(TQuery query) => true;

    protected virtual DataTable CreateDataTable(TQuery query) => query.GetNewDataTables().First();

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
    /// Adds the current reader record to the result table.
    /// </summary>
    protected abstract void AddRow(DataTable table, SqlDataReader reader);

    private void SendQueryResult(ReportQueryReceivedEventArgs args, DataTable result)
    {
        if (result.Rows.Count == 0)
        {
            return;
        }

        DataSet set = new();
        set.Tables.Add(result);
        Engine.ReportManager.SendQueryResult(args.MessageId, new ReportQueryResults(args.Query.ReportQueryType)
        {
            Results = set,
            QuerySource = args.QuerySource,
            ResultSource = Role.Guid,
            Succeeded = true,
            WaitForCompletion = false
        });
    }
}
