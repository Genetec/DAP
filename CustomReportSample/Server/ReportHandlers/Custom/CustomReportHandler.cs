// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers.Custom;

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Sdk;
using Sdk.Entities;
using Sdk.Queries;

public class CustomReportHandler : ReportHandler<CustomQuery, CustomReportRecord>
{
    public CustomReportHandler(IEngine engine, Role role) : base(engine, role)
    {
    }

    // Ensure that the custom report handler only handles the custom report with the specified identifier
    protected override bool IsQuerySupported(CustomQuery query)
    {
        return query.CustomReportId == CustomReportId.Value;
    }

    protected override DataTable CreateDataTable(CustomQuery query)
    {
        var table = new DataTable();

        table.Columns.Add(CustomReportColumnName.Value, typeof(double));
        table.Columns.Add(CustomReportColumnName.EventTimestamp, typeof(DateTime));
        table.Columns.Add(CustomReportColumnName.Message, typeof(string));
        table.Columns.Add(CustomReportColumnName.SourceId, typeof(Guid));
        table.Columns.Add(CustomReportColumnName.EventId, typeof(int));

        return table;
    }
   
    protected override void FillDataRow(DataRow row, CustomReportRecord record)
    {
        row[CustomReportColumnName.Value] = record.Value;
        row[CustomReportColumnName.EventTimestamp] = record.EventTimestamp;
        row[CustomReportColumnName.Message] = record.Message;
        row[CustomReportColumnName.SourceId] = record.SourceId;
        row[CustomReportColumnName.EventId] = record.EventId;
    }

    protected override async IAsyncEnumerable<CustomReportRecord> GetRecordsAsync(CustomQuery query)
    {
        // Deserialize the custom report filter data
        CustomReportFilterData filter = CustomReportFilterData.Deserialize(query.FilterData);

        // TODO: Implement the logic to retrieve the custom report records
        // Consider implementing batched database queries or paginated API calls for large datasets
        // to avoid loading all data into memory at once.

        await Task.Yield(); // Simulates asynchronous work (remove this in actual implementation)

        // This is an example of how to return a record
        yield return new CustomReportRecord
        {
            EventId = EventType.CustomEvent,
            EventTimestamp = DateTime.Now,
            Message = filter.Message,
            SourceId = SdkGuids.Administrator,
            Value = 1
        };
    }
}