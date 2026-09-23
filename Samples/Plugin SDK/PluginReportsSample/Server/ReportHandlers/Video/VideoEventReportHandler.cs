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

public class VideoEventReportHandler : DatabaseReportHandler<VideoEventQuery, VideoEvent>
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

    protected override VideoEvent MapRecord(SqlDataReader reader)
        => new()
        {
            EventTime = reader.GetUtcDateTime(Columns.EventTime),
            CameraGuid = reader.GetGuid(Columns.CameraGuid),
            ArchiveSourceGuid = reader.GetGuid(Columns.ArchiveSourceGuid),
            EventType = (EventType)reader.GetInt32(Columns.EventType),
            Value = (uint)reader.GetInt64(Columns.Value),
            Notes = reader.GetStringOrNull(Columns.Notes),
            XmlData = reader.GetStringOrNull(Columns.XmlData),
            Capabilities = (uint)reader.GetInt64(Columns.Capabilities),
            TimeZone = reader.GetString(Columns.TimeZone),
            Thumbnail = reader.GetBytesOrNull(Columns.Thumbnail)
        };

    protected override void FillDataRow(DataRow row, VideoEvent record)
    {
        row[VideoEventQuery.CameraGuidColumnName] = record.CameraGuid;
        row[VideoEventQuery.ArchiveSourceGuidColumnName] = record.ArchiveSourceGuid;
        row[VideoEventQuery.EventTimeColumnName] = record.EventTime;
        row[VideoEventQuery.EventTypeColumnName] = (uint)record.EventType;
        row[VideoEventQuery.ValueColumnName] = record.Value;
        row[VideoEventQuery.NotesColumnName] = record.Notes;
        row[VideoEventQuery.XmlDataColumnName] = record.XmlData;
        row[VideoEventQuery.CapabilitiesColumnName] = record.Capabilities;
        row[VideoEventQuery.TimeZoneColumnName] = record.TimeZone;
        row[VideoEventQuery.ThumbnailColumnName] = record.Thumbnail;
    }
}
