// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Genetec.Sdk.Diagnostics.Logging.Core;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Sdk;
using Sdk.Queries;
using Sdk.Workspace;
using Sdk.Workspace.Components.TimelineProvider;
using Sdk.Workspace.Pages.Contents;

public class AlarmTimelineProvider : TimelineProvider, IDisposable
{
    private readonly Logger m_logger;
    private readonly Workspace m_workspace;

    public AlarmTimelineProvider(Workspace workspace)
    {
        m_logger = Logger.CreateInstanceLogger(this);
        m_workspace = workspace;
        m_workspace.Sdk.AlarmAcknowledged += OnAlarmAcknowledged;
    }

    public override void Query(ContentGroup contentGroup, DateTime startTime, DateTime endTime)
    {
        _ = Task.Run(async () =>
        {
            try
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
            }
            catch (Exception ex)
            {
                m_logger.TraceError(ex, $"Failed to query alarm timeline events: {ex.Message}");
            }
            finally
            {
                OnQueryCompleted();
            }
        });
    }

    private void OnAlarmAcknowledged(object sender, AlarmAcknowledgedEventArgs e)
    {
        foreach (AlarmTimelineEvent timelineEvent in GetEvents().OfType<AlarmTimelineEvent>()
                     .Where(t => t.AlarmGuid == e.AlarmGuid && t.InstanceId == e.InstanceId)
                     .ToList())
        {
            RemoveEvent(timelineEvent);
        }
    }

    public void Dispose()
    {
        m_workspace.Sdk.AlarmAcknowledged -= OnAlarmAcknowledged;
        m_logger.Dispose();
    }
}
