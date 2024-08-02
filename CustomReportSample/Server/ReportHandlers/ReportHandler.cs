// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Genetec.Dap.CodeSamples;
    using Genetec.Sdk;
    using Genetec.Sdk.Diagnostics.Logging.Core;
    using Genetec.Sdk.Entities;
    using Genetec.Sdk.EventsArgs;
    using Genetec.Sdk.Queries;

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

        public async Task HandleAsync(ReportQueryReceivedEventArgs args, CancellationToken token)
        {
            if (args.Query is TQuery query && IsQuerySupported(query))
            {
                IAsyncEnumerable<TRecord> records = GetRecordsAsync(query);

                await foreach (IAsyncEnumerable<TRecord> batch in records.Buffer(GetBatchSize()).WithCancellation(token))
                {
                    DataTable table = CreateDataTable(query);
                    await ProcessBatch(table, batch);
                    SendQueryResult(args, table);
                }
            }
        }

        protected virtual bool IsQuerySupported(TQuery query) => true;

        protected virtual DataTable CreateDataTable(TQuery query)
        {
            // Default implementation for most reports
            return query.GetNewDataTables().First();
        }

        protected virtual async Task ProcessBatch(DataTable table, IAsyncEnumerable<TRecord> batch)
        {
            await foreach (TRecord record in batch)
            {
                DataRow row = table.NewRow();
                FillDataRow(row, record);
                table.Rows.Add(row);
            }
        }

        protected abstract void FillDataRow(DataRow row, TRecord record);

        protected abstract IAsyncEnumerable<TRecord> GetRecordsAsync(TQuery query);

        protected virtual int GetBatchSize() => 100;

        protected void SendQueryResult(ReportQueryReceivedEventArgs args, DataTable result)
        {
            try
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
            catch (SdkException ex) when (ex.ErrorCode == SdkError.InvalidOperation)
            {
                // Handle or log the exception as needed
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Logger.Dispose();
            }
        }
    }
}
