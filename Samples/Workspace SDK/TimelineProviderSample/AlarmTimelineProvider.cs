// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Sdk;
using Sdk.Queries;
using Sdk.Workspace;
using Sdk.Workspace.Components.TimelineProvider;
using Sdk.Workspace.Pages.Contents;

public class AlarmTimelineProvider : TimelineProvider
{
    private readonly Workspace m_workspace;

    public AlarmTimelineProvider(Workspace workspace)
    {
        m_workspace = workspace;

        workspace.Sdk.AlarmAcknowledged += OnAlarmAcknowledged;

        void OnAlarmAcknowledged(object sender, AlarmAcknowledgedEventArgs e)
        {
            foreach (AlarmTimelineEvent timelineEvent in GetEvents().OfType<AlarmTimelineEvent>()
                         .Where(timelineEvent => timelineEvent.AlarmGuid == e.AlarmGuid && timelineEvent.InstanceId == e.InstanceId))
            {
                RemoveEvent(timelineEvent);
            }
        }
    }

    public override void Query(ContentGroup contentGroup, DateTime startTime, DateTime endTime)
    {
        Task.Run(async () =>
        {
            var query = (AlarmActivityQuery)m_workspace.Sdk.ReportManager.CreateReportQuery(ReportType.AlarmActivity);
            query.TriggeredTimeRange.SetTimeRange(startTime, endTime);
            query.States.Add(AlarmState.Active);
            query.States.Add(AlarmState.AknowledgeRequired);
            query.States.Add(AlarmState.SourceConditionInvestigating);

            QueryCompletedEventArgs result = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            var events = result.Data.AsEnumerable().Select(row =>
            {
                var alarmGuid = row.Field<Guid>(AlarmActivityQuery.AlarmColumnName);
                var instanceId = row.Field<int>(AlarmActivityQuery.InstanceIdColumnName);
                var triggerTime = row.Field<DateTime>(AlarmActivityQuery.TriggerTimeColumnName);
                    
                return new AlarmTimelineEvent(alarmGuid, instanceId, triggerTime);
            });

            InsertEvents(events);
        });
    }
}