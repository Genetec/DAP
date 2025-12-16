// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Diagnostics.Logging.Core;
using Genetec.Sdk.Entities;
using Genetec.Sdk.EventsArgs;
using Genetec.Sdk.Plugin.Queries.Rows;
using Genetec.Sdk.Plugin.Queries.Rows.Extensions;
using Genetec.Sdk.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public abstract class ReportHandler<TQuery, TRecord> : IReportHandler, IDisposable where TQuery : ReportQuery
{
    protected ReportHandler(IEngine engine, Role role)
    {
        Logger = Logger.CreateInstanceLogger(this);
        Engine = engine;
        Role = role;
    }

    protected Logger Logger { get; }
    protected IEngine Engine { get; }
    protected Role Role { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task<ReportError> HandleAsync(ReportQueryReceivedEventArgs args, CancellationToken token)
    {
        if (args.Query is TQuery query && IsQuerySupported(query))
        {
            IAsyncEnumerable<TRecord> records = GetRecordsAsync(query);
            int totalSent = 0;
            int maxResults = args.Query.MaximumResultCount;

            await foreach (IAsyncEnumerable<TRecord> batch in records.Buffer(GetBatchSize()).WithCancellation(token))
            {
                token.ThrowIfCancellationRequested();

                DataTable table = CreateDataTable(query);
                await ProcessBatch(table, batch);
                SendQueryResult(args, table);

                totalSent += table.Rows.Count;

                if (maxResults > 0 && totalSent > maxResults)
                {
                    return ReportError.TooManyResults;
                }
            }
        }

        return ReportError.None;
    }

    protected virtual bool IsQuerySupported(TQuery query) => true;

    protected virtual DataTable CreateDataTable(TQuery query)
    {
        return query.GetNewDataTables().First();
    }

    protected virtual async Task ProcessBatch(DataTable table, IAsyncEnumerable<TRecord> batch)
    {
        await foreach (TRecord record in batch)
        {
            if (record is IRow row)
            {
                table.AddIRow(row);
            }
            else
            {
                DataRow dataRow = table.NewRow();
                FillDataRow(dataRow, record);
                table.Rows.Add(dataRow);
            }
        }
    }

    protected virtual void FillDataRow(DataRow row, TRecord record)
    {
    }

    protected abstract IAsyncEnumerable<TRecord> GetRecordsAsync(TQuery query);

    protected virtual int GetBatchSize()
    {
        return 100; // Default batch size for most reports
    }

    protected void SendQueryResult(ReportQueryReceivedEventArgs args, DataTable result)
    {
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

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Logger.Dispose();
        }
    }
}