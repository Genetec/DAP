// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Sdk;
using Sdk.Events.AccessPoint;
using Sdk.Queries;
using Sdk.Queries.AccessControl;
using Sdk.Workspace;
using Sdk.Workspace.Components.TimelineProvider;
using Sdk.Workspace.Pages.Contents;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

public class AccessControlTimelineProvider : TimelineProvider, IDisposable
{
    private readonly Workspace m_workspace;

    public AccessControlTimelineProvider(Workspace workspace)
    {
        m_workspace = workspace;
        m_workspace.Sdk.EventReceived += OnEventReceived;
    }

    public override void Query(ContentGroup contentGroup, DateTime startTime, DateTime endTime)
    {
        Task.Run(async () =>
        {
            var query = (CardholderActivityQuery)m_workspace.Sdk.ReportManager.CreateReportQuery(ReportType.CardholderActivity);
            query.TimeRange.SetTimeRange(startTime, endTime);
            query.Events.Clear();
            query.Events.Add(EventType.AccessGranted);
            query.Events.Add(EventType.AccessRefused);

            QueryCompletedEventArgs result = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            var events = result.Data.AsEnumerable().Select(row =>
            {
                var timestamp = row.Field<DateTime>(AccessControlReportQuery.TimestampColumnName);
                var cardholderGuid = row.Field<Guid>(AccessControlReportQuery.CardholderGuidColumnName);
                var eventType = row.Field<EventType>(AccessControlReportQuery.EventTypeColumnName);

                return new AccessTimelineEvent(cardholderGuid, timestamp, eventType == EventType.AccessGranted);
            });

            InsertEvents(events);
            OnQueryCompleted();
        });
    }

    private void OnEventReceived(object sender, EventReceivedEventArgs e)
    {
        switch (e.EventType)
        {
            case EventType.AccessGranted:
            case EventType.AccessRefused:
                if (e.Event is AccessEvent accessEvent)
                {
                    var timelineEvent = new AccessTimelineEvent(
                        accessEvent.Cardholder,
                        accessEvent.Timestamp,
                        accessEvent.Type == EventType.AccessGranted);
                    InsertEvent(timelineEvent);
                }
                break;
        }
    }

    public void Dispose()
    {
        m_workspace.Sdk.EventReceived -= OnEventReceived;
    }
}
