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
using System.Drawing;
using System.IO;
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

        table.Columns.Add(CustomReportColumnName.SourceId, typeof(Guid));
        table.Columns.Add(CustomReportColumnName.EventId, typeof(int));
        table.Columns.Add(CustomReportColumnName.Message, typeof(string));
        table.Columns.Add(CustomReportColumnName.Numeric, typeof(int));
        table.Columns.Add(CustomReportColumnName.EventTimestamp, typeof(DateTime));
        table.Columns.Add(CustomReportColumnName.Decimal, typeof(decimal));
        table.Columns.Add(CustomReportColumnName.Boolean, typeof(bool));
        table.Columns.Add(CustomReportColumnName.Picture, typeof(byte[])).AllowDBNull = true;
        table.Columns.Add(CustomReportColumnName.Duration, typeof(TimeSpan));
        table.Columns.Add(CustomReportColumnName.Hidden, typeof(string));

        return table;
    }

    protected override void FillDataRow(DataRow row, CustomReportRecord record)
    {
        row[CustomReportColumnName.SourceId] = record.SourceId;
        row[CustomReportColumnName.EventId] = record.EventId;
        row[CustomReportColumnName.Message] = record.Message;
        row[CustomReportColumnName.Numeric] = record.Numeric;
        row[CustomReportColumnName.EventTimestamp] = record.EventTimestamp;
        row[CustomReportColumnName.Decimal] = record.Decimal;
        row[CustomReportColumnName.Boolean] = record.Boolean;
        row[CustomReportColumnName.Picture] = record.Picture;
        row[CustomReportColumnName.Duration] = record.Duration;
        row[CustomReportColumnName.Hidden] = record.Hidden;
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

        foreach (var guid in query.QueryEntities)
        {
            yield return new CustomReportRecord
            {
                SourceId = guid,
                EventTimestamp = query.TimeRange.DateTime,
                Message = filter.Message,
                Numeric = filter.NumericValue,
                Decimal = filter.DecimalValue,
                Boolean = filter.Enabled,
                Duration = query.TimeRange.TimeSpan,
                Picture = ConvertImageToByteArray((Engine.GetEntity(guid) as Cardholder)?.Picture),
                Hidden = "This is the content of the hidden field",
                EventId = -filter.CustomEvent ?? 0
            };
        }

        byte[] ConvertImageToByteArray(Image image)
        {
            if (image is null) return null;
            using MemoryStream stream = new();
            image.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
            return stream.ToArray();
        }
    }
}