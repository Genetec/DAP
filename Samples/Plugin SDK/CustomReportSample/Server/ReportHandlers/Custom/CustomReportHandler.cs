// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers.Custom;

using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Sdk;
using Sdk.Entities;
using Sdk.EventsArgs;
using Sdk.Queries;

public sealed class CustomReportHandler
{
    private const int BatchSize = 100;

    private readonly IEngine m_engine;
    private readonly Role m_role;

    public CustomReportHandler(IEngine engine, Role role)
    {
        m_engine = engine;
        m_role = role;
    }

    public async Task<ReportError> HandleAsync(ReportQueryReceivedEventArgs args, CancellationToken cancellationToken)
    {
        if (args.Query is not CustomQuery query || query.CustomReportId != CustomReportId.Value)
        {
            return ReportError.None;
        }

        await Task.Yield();

        CustomReportFilterData filter = CustomReportFilterData.Deserialize(query.FilterData);
        DataTable table = CreateDataTable();
        int totalRows = 0;

        foreach (Guid entityId in query.QueryEntities)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (query.MaximumResultCount > 0 && totalRows == query.MaximumResultCount)
            {
                SendQueryResult(args, table);
                return ReportError.TooManyResults;
            }

            DataRow row = table.NewRow();
            row[CustomReportColumnName.SourceId] = entityId;
            row[CustomReportColumnName.EventId] = filter.CustomEvent.HasValue ? -filter.CustomEvent.Value : 0;
            row[CustomReportColumnName.Message] = filter.Message;
            row[CustomReportColumnName.Numeric] = filter.NumericValue;
            row[CustomReportColumnName.EventTimestamp] = query.TimeRange.DateTime;
            row[CustomReportColumnName.Decimal] = filter.DecimalValue;
            row[CustomReportColumnName.Boolean] = filter.Enabled;
            row[CustomReportColumnName.Picture] = (object)ConvertImageToByteArray((m_engine.GetEntity(entityId) as Cardholder)?.Picture) ?? DBNull.Value;
            row[CustomReportColumnName.Duration] = query.TimeRange.TimeSpan;
            row[CustomReportColumnName.Hidden] = "This is the content of the hidden field";
            table.Rows.Add(row);
            totalRows++;

            if (table.Rows.Count == BatchSize)
            {
                SendQueryResult(args, table);
                table = CreateDataTable();
            }
        }

        SendQueryResult(args, table);
        return ReportError.None;
    }

    private static DataTable CreateDataTable()
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

    private static byte[] ConvertImageToByteArray(Image image)
    {
        if (image is null)
        {
            return null;
        }

        using var stream = new MemoryStream();
        image.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
        return stream.ToArray();
    }

    private void SendQueryResult(ReportQueryReceivedEventArgs args, DataTable table)
    {
        if (table.Rows.Count == 0)
        {
            return;
        }

        var results = new DataSet();
        results.Tables.Add(table);
        m_engine.ReportManager.SendQueryResult(args.MessageId, new ReportQueryResults(args.Query.ReportQueryType)
        {
            Results = results,
            QuerySource = args.QuerySource,
            ResultSource = m_role.Guid,
            Succeeded = true,
            WaitForCompletion = false
        });
    }
}
