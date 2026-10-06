// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.EventsArgs.Query;
using Genetec.Sdk.Queries;
using Genetec.Sdk.Queries.AsyncResult;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        // The plugin returns the time range length in its Duration column.
        query.TimeRange.SetTimeRange(TimeSpan.FromMinutes(30));
        query.FilterData = new CustomReportFilterData
        {
            Enabled = true,
            DecimalValue = 3.14m,
            Message = "Hello, World!",
            NumericValue = 42
        }.Serialize();

        // Load cardholders into the entity cache
        await LoadEntities(engine, token, EntityType.Cardholder);

        // Add cardholders to query
        query.QueryEntities.AddRange(engine.GetEntities(EntityType.Cardholder).Take(10).Select(entity => entity.Guid));

        Console.WriteLine("\nExecuting custom report query...");
        Console.WriteLine("Press Ctrl+C to cancel at any time\n");
        try
        {
            ReportQueryAsyncResult result = await engine.ReportManager.QueryAsync(query, token);

            if (result.Results.Any())
            {
                foreach (IReportQueryResultReceivedEventArgs queryResult in result.Results)
                {
                    foreach (DataRow row in queryResult.ResultContainer.DataSet.Tables[0].Rows)
                    {
                        DisplayCustomReportRecord(row);
                    }
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

    private void DisplayCustomReportRecord(DataRow row)
    {
        byte[] picture = row.Field<byte[]>(CustomReportColumnName.Picture);
        string hidden = row.Field<string>(CustomReportColumnName.Hidden);

        Console.WriteLine("\n--- Custom Report Record ---");
        Console.WriteLine($"Source ID: {row.Field<Guid>(CustomReportColumnName.SourceId)}");
        Console.WriteLine($"Event ID:  {row.Field<int>(CustomReportColumnName.EventId)}");
        Console.WriteLine($"Message:   {row.Field<string>(CustomReportColumnName.Message)}");
        Console.WriteLine($"Numeric:   {row.Field<int>(CustomReportColumnName.Numeric)}");
        Console.WriteLine($"Timestamp: {row.Field<DateTime>(CustomReportColumnName.EventTimestamp):yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Decimal:   {row.Field<decimal>(CustomReportColumnName.Decimal):F2}");
        Console.WriteLine($"Boolean:   {row.Field<bool>(CustomReportColumnName.Boolean)}");
        Console.WriteLine($"Picture:   {(picture?.Length > 0 ? $"{picture.Length} bytes" : "No picture")}");
        Console.WriteLine($"Duration:  {row.Field<TimeSpan>(CustomReportColumnName.Duration):hh\\:mm\\:ss}");
        Console.WriteLine($"Hidden:    {(string.IsNullOrEmpty(hidden) ? "N/A" : hidden)}");
        Console.WriteLine();
    }
}
