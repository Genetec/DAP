// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples.Server.ReportHandlers.Custom;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;
using Genetec.Sdk.Queries.AsyncResult;

namespace Genetec.Dap.CodeSamples;

/// <summary>
/// For this sample to work, ensure the CustomReportSample plugin is installed and running in your Security Center environment.
/// </summary>
public class CustomReportQuerySample : SampleBase
{
    private const string s_customReportSamplePluginGuid = "4E8BB3F7-0D41-4F4C-A430-0B6EE7478CBE"; // TODO: Replace with the actual GUID of your plugin

    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load roles into the entity cache
        await LoadEntities(engine, token, EntityType.Role);

        // Find the CustomReportSample plugin role from the entity cache
        Role plugin = engine.GetEntities(EntityType.Role).OfType<Role>().FirstOrDefault(role => role.Type == RoleType.Plugin && role.SubType == new Guid(s_customReportSamplePluginGuid));
        if (plugin?.IsOnline != true)
        {
            Console.WriteLine("The CustomReportSample plugin is not online.");
        }

        var query = (CustomQuery)engine.ReportManager.CreateReportQuery(ReportType.Custom);
        query.CustomReportId = CustomReportId.Value; // Custom report identifier
        query.FilterData = new CustomReportFilterData
        {
            Enabled = true,
            DecimalValue = 3.14m,
            Message = "Hello, World!",
            NumericValue = 42,
            Duration = TimeSpan.FromMinutes(30)
        }.Serialize();

        // Load cardholders into the entity cache
        await LoadEntities(engine, token, EntityType.Cardholder);

        // Add cardholders to query
        query.QueryEntities.AddRange(engine.GetEntities(EntityType.Cardholder).Select(entity => entity.Guid));

        Console.WriteLine("\nExecuting custom report query...");
        Console.WriteLine("Press Ctrl+C to cancel at any time\n");
        try
        {
            ReportQueryAsyncResult result = await engine.ReportManager.QueryAsync(query, token);

            if (result.Results.Any())
            {
                IEnumerable<CustomReportRecord> records = result.Results[0].ResultContainer.DataSet.Tables[0].AsEnumerable().Select(CreateCustomReportRecord);
                foreach (CustomReportRecord record in records)
                {
                    DisplayCustomReportRecord(record);
                }
            }
            else
            {
                Console.WriteLine("No records returned from the custom report query. Ensure the CustomReportSample plugin is online.");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\nQuery cancelled by user");
        }
    }

    private void DisplayCustomReportRecord(CustomReportRecord record)
    {
        Console.WriteLine("\n--- Custom Report Record ---");
        Console.WriteLine($"Source ID: {record.SourceId}");
        Console.WriteLine($"Event ID:  {record.EventId}");
        Console.WriteLine($"Message:   {record.Message}");
        Console.WriteLine($"Numeric:   {record.Numeric}");
        Console.WriteLine($"Timestamp: {record.EventTimestamp:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Decimal:   {record.Decimal:F2}");
        Console.WriteLine($"Boolean:   {record.Boolean}");
        Console.WriteLine($"Picture:   {(record.Picture?.Length > 0 ? $"{record.Picture.Length} bytes" : "No picture")}");
        Console.WriteLine($"Duration:  {record.Duration:hh\\:mm\\:ss}");
        Console.WriteLine($"Hidden:    {(string.IsNullOrEmpty(record.Hidden) ? "N/A" : record.Hidden)}");
        Console.WriteLine();
    }

    private CustomReportRecord CreateCustomReportRecord(DataRow row) => new()
    {
        SourceId = row.Field<Guid>(CustomReportColumnName.SourceId),
        EventId = row.Field<int>(CustomReportColumnName.EventId),
        Message = row.Field<string>(CustomReportColumnName.Message),
        Numeric = row.Field<int>(CustomReportColumnName.Numeric),
        EventTimestamp = row.Field<DateTime>(CustomReportColumnName.EventTimestamp),
        Decimal = row.Field<decimal>(CustomReportColumnName.Decimal),
        Boolean = row.Field<bool>(CustomReportColumnName.Boolean),
        Picture = row.Field<byte[]>(CustomReportColumnName.Picture),
        Duration = row.Field<TimeSpan>(CustomReportColumnName.Duration),
        Hidden = row.Field<string>(CustomReportColumnName.Hidden)
    };
}