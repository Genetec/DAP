// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

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