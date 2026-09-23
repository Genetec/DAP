// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers.Video;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries.Video;
using Columns = VideoEventTable.Columns;

public class VideoEventReportHandler : DatabaseReportHandler<VideoEventQuery>
{
    public VideoEventReportHandler(IEngine engine, Role role, SampleDatabaseManager databaseManager) : base(engine, role, databaseManager)
    {
    }

    protected override string TableName => VideoEventTable.Name;

    protected override string SelectColumns =>
        $"{Columns.EventTime}, {Columns.CameraGuid}, {Columns.ArchiveSourceGuid}, {Columns.EventType}, {Columns.Value}, " +
        $"{Columns.Notes}, {Columns.XmlData}, {Columns.Capabilities}, {Columns.TimeZone}, {Columns.Thumbnail}";

    protected override string TimestampColumn => Columns.EventTime;

    protected override Task AddFiltersAsync(ICollection<string> conditions, SqlCommand command, VideoEventQuery query)
    {
        SqlFilterBuilder.AddIntFilter(conditions, Columns.EventType, query.Events.Where(eventType => eventType != EventType.None).Select(eventType => (int)eventType).ToList());
        SqlFilterBuilder.AddGuidFilter(conditions, command, Columns.CameraGuid, query.Cameras, "Camera");
        return Task.CompletedTask;
    }

    protected override void AddRow(DataTable table, SqlDataReader reader)
    {
        DataRow row = table.NewRow();
        row[VideoEventQuery.CameraGuidColumnName] = reader.GetGuid(Columns.CameraGuid);
        row[VideoEventQuery.ArchiveSourceGuidColumnName] = reader.GetGuid(Columns.ArchiveSourceGuid);
        row[VideoEventQuery.EventTimeColumnName] = reader.GetUtcDateTime(Columns.EventTime);
        row[VideoEventQuery.EventTypeColumnName] = (uint)reader.GetInt32(Columns.EventType);
        row[VideoEventQuery.ValueColumnName] = (uint)reader.GetInt64(Columns.Value);
        row[VideoEventQuery.NotesColumnName] = reader.GetStringOrNull(Columns.Notes);
        row[VideoEventQuery.XmlDataColumnName] = reader.GetStringOrNull(Columns.XmlData);
        row[VideoEventQuery.CapabilitiesColumnName] = (uint)reader.GetInt64(Columns.Capabilities);
        row[VideoEventQuery.TimeZoneColumnName] = reader.GetString(Columns.TimeZone);
        row[VideoEventQuery.ThumbnailColumnName] = reader.GetBytesOrNull(Columns.Thumbnail);
        table.Rows.Add(row);
    }
}
