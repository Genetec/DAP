// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;
using Microsoft.Data.SqlClient;

public sealed class CustomEventsReportHandler : DatabaseReportHandler<CustomQuery>
{
    public CustomEventsReportHandler(IEngine engine, Role role, SampleDatabaseManager databaseManager)
        : base(engine, role, databaseManager) { }

    protected override bool IsQuerySupported(CustomQuery query) => query.CustomReportId == CustomEventReport.Id;
    protected override string TableName => "dbo.CustomEvents";
    protected override string SelectColumns => "EventTimestamp, CustomEventId, SourceGuid, Message";

    protected override Task AddFiltersAsync(ICollection<string> conditions, SqlCommand command, CustomQuery query)
    {
        var filter = CustomEventFilterData.Deserialize(query.FilterData);
        var eventIds = new HashSet<int>(CustomEventReport.GetDefinitions(Engine).Select(item => item.Id));
        if (filter.CustomEventId.HasValue) eventIds.IntersectWith(new[] { filter.CustomEventId.Value });
        if (query.CustomEvents.Count > 0) eventIds.IntersectWith(query.CustomEvents);
        if (query.EventTypes.Any(item => item != EventType.None) && query.CustomEvents.Count == 0)
            eventIds.Clear(); // This report contains custom events only.

        if (eventIds.Count == 0) conditions.Add("1 = 0");
        else SqlFilterBuilder.AddIntFilter(conditions, "CustomEventId", eventIds);

        if (query.QueryEntities.Count > 0)
        {
            var entities = new HashSet<Guid>(query.QueryEntities);
            entities.ExceptWith(query.ExcludedExpansionEntities);
            if (entities.Count == 0) conditions.Add("1 = 0");
            else SqlFilterBuilder.AddGuidFilter(conditions, command, "SourceGuid", entities, "Source");
        }
        else if (query.ExcludedExpansionEntities.Count > 0)
        {
            string parameters = SqlFilterBuilder.AddGuidList(command, query.ExcludedExpansionEntities.ToList(), "Excluded");
            conditions.Add($"SourceGuid NOT IN ({parameters})");
        }

        SqlFilterBuilder.AddStringFilter(conditions, command, "Message", filter.Message, StringSearchMode.Contains, "@Message");
        return Task.CompletedTask;
    }

    protected override DataTable CreateDataTable(CustomQuery query)
    {
        var table = new DataTable("CustomEvents");
        table.Columns.Add(CustomEventReport.Source, typeof(Guid));
        table.Columns.Add(new DataColumn(CustomEventReport.Timestamp, typeof(DateTime)) { DateTimeMode = DataSetDateTime.Utc });
        table.Columns.Add(CustomEventReport.Event, typeof(int));
        table.Columns.Add(CustomEventReport.Message, typeof(string));
        return table;
    }

    protected override void AddRow(DataTable table, SqlDataReader reader)
    {
        DataRow row = table.NewRow();
        row[CustomEventReport.Source] = reader.GetGuid("SourceGuid");
        row[CustomEventReport.Timestamp] = reader.GetUtcDateTime("EventTimestamp");
        row[CustomEventReport.Event] = -reader.GetInt32("CustomEventId"); // The native Event column resolves negative IDs as custom events.
        row[CustomEventReport.Message] = reader.GetString("Message");
        table.Rows.Add(row);
    }
}
