// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.EventsArgs;
using Genetec.Sdk.Plugin.Queries.Rows;
using Genetec.Sdk.Plugin.Queries.Rows.Extensions;
using Genetec.Sdk.Queries;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public abstract class ReportHandler<TQuery, TRecord> : IReportHandler where TQuery : ReportQuery
{
    protected ReportHandler(IEngine engine, Role role)
    {
        Engine = engine;
        Role = role;
    }

    protected IEngine Engine { get; }
    protected Role Role { get; }

    public async Task<ReportError> HandleAsync(ReportQueryReceivedEventArgs args, CancellationToken token)
    {
        if (args.Query is TQuery query && IsQuerySupported(query))
        {
            IAsyncEnumerable<TRecord> records = GetRecordsAsync(query, token);
            int totalSent = 0;
            int maxResults = args.Query.MaximumResultCount;

            await foreach (IReadOnlyList<TRecord> batch in records.Buffer(100).WithCancellation(token))
            {
                token.ThrowIfCancellationRequested();

                DataTable table = CreateDataTable(query);
                foreach (TRecord record in batch)
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

                // The database reads one extra row to detect overflow. Do not send that row.
                bool tooManyResults = maxResults > 0 && table.Rows.Count > maxResults - totalSent;
                if (tooManyResults)
                {
                    while (table.Rows.Count > maxResults - totalSent)
                    {
                        table.Rows.RemoveAt(table.Rows.Count - 1);
                    }
                }

                if (table.Rows.Count > 0)
                {
                    SendQueryResult(args, table);
                }

                totalSent += table.Rows.Count;

                if (tooManyResults)
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

    protected virtual void FillDataRow(DataRow row, TRecord record)
    {
    }

    protected abstract IAsyncEnumerable<TRecord> GetRecordsAsync(TQuery query, CancellationToken cancellationToken);

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

}
