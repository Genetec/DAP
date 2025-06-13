// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Dap.CodeSamples.Server.ReportHandlers.Custom;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;
using Genetec.Sdk.Queries.AsyncResult;

const string server = "localhost";
const string username = "admin";
const string password = "";
const string customReportSamplePluginGuid = "4E8BB3F7-0D41-4F4C-A430-0B6EE7478CBE";

SdkResolver.Initialize();

await RunSample();

Console.Write("Press any key to exit...");
Console.ReadKey(true);

async Task RunSample()
{
    using var engine = new Engine();

    engine.LogonStatusChanged += (_, args) => Console.Write($"\rConnection status: {args.Status}".PadRight(Console.WindowWidth));
    engine.LogonFailed += (_, args) => Console.WriteLine($"\rError: {args.FormattedErrorMessage}".PadRight(Console.WindowWidth));
    engine.LoggedOn += (_, args) => Console.WriteLine($"\rConnected to {args.ServerName}".PadRight(Console.WindowWidth));

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state != ConnectionStateCode.Success)
        return;

    await LoadRole();

    // Find the CustomReportSample plugin role
    Role plugin = engine.GetEntities(EntityType.Role).OfType<Role>().FirstOrDefault(role => role.Type == RoleType.Plugin && role.SubType == new Guid(customReportSamplePluginGuid));
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

    // Load cardholders into the engine's cache
    await LoadCardholders();

    // Add cardholders to query
    query.QueryEntities.AddRange(engine.GetEntities(EntityType.Cardholder).Select(entity => entity.Guid));

    Console.WriteLine("\nExecuting custom report query...");
    Console.WriteLine("Press Ctrl+C to cancel at any time\n");
    try
    {
        using CancellationTokenSource cancellationTokenSource = CreateCancellationTokenSource();
        ReportQueryAsyncResult result = await engine.ReportManager.QueryAsync(query, cancellationTokenSource.Token);

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

    Task LoadRole()
    {
        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Role);
        return Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    }

    Task LoadCardholders()
    {
        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Cardholder);
        query.MaximumResultCount = 10; // Limit to 10 cardholders
        return Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    }

    CancellationTokenSource CreateCancellationTokenSource()
    {
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            Console.WriteLine("\nCancelling operation...");
            e.Cancel = true;
            cts.Cancel();
        };
        return cts;
    }

    void DisplayCustomReportRecord(CustomReportRecord record)
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

    CustomReportRecord CreateCustomReportRecord(DataRow row)
    {
        return new CustomReportRecord
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
}