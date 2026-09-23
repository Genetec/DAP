// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers.AccessControl;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries.AccessControl;
using Columns = AccessControlEventTable.Columns;

public class AccessControlReportHandler : DatabaseReportHandler<AccessControlReportQuery>
{
    public AccessControlReportHandler(IEngine engine, Role role, SampleDatabaseManager databaseManager) : base(engine, role, databaseManager)
    {
    }

    protected override string TableName => AccessControlEventTable.Name;

    protected override string SelectColumns =>
        $"{Columns.EventTimestamp}, {Columns.EventType}, {Columns.UnitGuid}, {Columns.DeviceGuid}, {Columns.APGuid}, {Columns.SourceGuid}, " +
        $"{Columns.CredentialGuid}, {Columns.CardholderGuid}, {Columns.Credential2Guid}, {Columns.TimeZone}, {Columns.OccurrencePeriod}, " +
        $"{Columns.AccessPointGroupGuid}, {Columns.CustomEventMessage}";

    protected override string TimestampColumn => Columns.EventTimestamp;

    protected override async Task AddFiltersAsync(ICollection<string> conditions, SqlCommand command, AccessControlReportQuery query)
    {
        // Event filter, remapped per report: the cardholder-centric reports use the Cardholder*
        // event variants, while the door-centric reports use the plain variants
        List<EventType> events = query.Events.Where(eventType => eventType != EventType.None).ToList();
        switch (query.ReportType)
        {
            case ReportType.CardholderActivity:
            case ReportType.VisitorActivity:
                Replace(events, EventType.AccessGranted, EventType.CardholderAccessGranted);
                Replace(events, EventType.AccessRefused, EventType.CardholderAccessRefused);
                break;

            case ReportType.AreaActivity:
            case ReportType.CredentialActivity:
                if (events.Contains(EventType.AccessGranted) && !events.Contains(EventType.CardholderAccessGranted))
                    events.Add(EventType.CardholderAccessGranted);

                if (events.Contains(EventType.AccessRefused) && !events.Contains(EventType.CardholderAccessRefused))
                    events.Add(EventType.CardholderAccessRefused);

                break;
        }

        List<int> eventTypes = events.Select(eventType => (int)eventType)
            .Concat(query.CustomEvents.Select(customEventId => -customEventId))
            .ToList();
        SqlFilterBuilder.AddIntFilter(conditions, Columns.EventType, eventTypes);

        // Entity filter: cardholder groups are expanded into their member cardholders,
        // and a record matches when any of its entity columns is a selected entity.
        // This handler serves several activity reports, so the selected entities can be
        // doors or areas (SourceGuid), cardholders, or credentials.
        if (query.QueryEntities.Count > 0 || query.IncludedExpansionEntities.Count > 0)
        {
            QueryEntitySelection selection = await QueryEntityExpander.ExpandAccessControlSelectionAsync(
                Engine,
                query.QueryEntities,
                query.IncludedExpansionEntities,
                query.ExcludedExpansionEntities);

            if (selection.IsUnrestricted)
            {
                if (selection.Excluded.Count > 0)
                {
                    string excluded = SqlFilterBuilder.AddGuidList(command, selection.Excluded, "ExcludedEntity");
                    conditions.Add($"({Columns.SourceGuid} NOT IN ({excluded}) AND " +
                        $"({Columns.UnitGuid} IS NULL OR {Columns.UnitGuid} NOT IN ({excluded})) AND " +
                        $"({Columns.DeviceGuid} IS NULL OR {Columns.DeviceGuid} NOT IN ({excluded})) AND " +
                        $"({Columns.APGuid} IS NULL OR {Columns.APGuid} NOT IN ({excluded})) AND " +
                        $"({Columns.CardholderGuid} IS NULL OR {Columns.CardholderGuid} NOT IN ({excluded})) AND " +
                        $"({Columns.CredentialGuid} IS NULL OR {Columns.CredentialGuid} NOT IN ({excluded})) AND " +
                        $"({Columns.Credential2Guid} IS NULL OR {Columns.Credential2Guid} NOT IN ({excluded})) AND " +
                        $"({Columns.AccessPointGroupGuid} IS NULL OR {Columns.AccessPointGroupGuid} NOT IN ({excluded})))");
                }
            }
            else if (selection.Included.Count > 0)
            {
                string included = SqlFilterBuilder.AddGuidList(command, selection.Included, "Entity");
                conditions.Add($"({Columns.SourceGuid} IN ({included}) OR {Columns.UnitGuid} IN ({included}) OR " +
                    $"{Columns.DeviceGuid} IN ({included}) OR {Columns.APGuid} IN ({included}) OR " +
                    $"{Columns.CardholderGuid} IN ({included}) OR {Columns.CredentialGuid} IN ({included}) OR " +
                    $"{Columns.Credential2Guid} IN ({included}) OR {Columns.AccessPointGroupGuid} IN ({included}))");
            }
            else
            {
                // A selected empty group, or a selection emptied by exclusions, matches no records.
                conditions.Add("1 = 0");
            }
        }
    }

    protected override void AddRow(DataTable table, SqlDataReader reader)
    {
        DataRow row = table.NewRow();
        row[AccessControlReportQuery.TimestampColumnName] = reader.GetUtcDateTime(Columns.EventTimestamp);
        row[AccessControlReportQuery.EventTypeColumnName] = (EventType)reader.GetInt32(Columns.EventType);
        row[AccessControlReportQuery.UnitGuidColumnName] = reader.GetGuidOrDefault(Columns.UnitGuid);
        row[AccessControlReportQuery.DeviceGuidColumnName] = reader.GetGuidOrDefault(Columns.DeviceGuid);
        row[AccessControlReportQuery.APGuidColumnName] = reader.GetGuidOrDefault(Columns.APGuid);
        row[AccessControlReportQuery.SourceGuidColumnName] = reader.GetGuid(Columns.SourceGuid);
        row[AccessControlReportQuery.CredentialGuidColumnName] = reader.GetGuidOrDefault(Columns.CredentialGuid);
        row[AccessControlReportQuery.CardholderGuidColumnName] = reader.GetGuidOrDefault(Columns.CardholderGuid);
        row[AccessControlReportQuery.Credential2GuidColumnName] = reader.GetGuidOrDefault(Columns.Credential2Guid);
        row[AccessControlReportQuery.TimeZoneColumnName] = reader.GetString(Columns.TimeZone);
        row[AccessControlReportQuery.OccurrencePeriodColumnName] = reader.GetInt32(Columns.OccurrencePeriod);
        row[AccessControlReportQuery.AccessPointGroupGuidColumnName] = reader.GetGuidOrDefault(Columns.AccessPointGroupGuid);
        row[AccessControlReportQuery.CustomEventMessageColumnName] = reader.GetStringOrNull(Columns.CustomEventMessage);
        table.Rows.Add(row);
    }

    // Replaces every occurrence of an event type in the list
    private static void Replace(List<EventType> events, EventType from, EventType to)
    {
        for (int i = 0; i < events.Count; i++)
        {
            if (events[i] == from)
            {
                events[i] = to;
            }
        }
    }
}
