// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using Genetec.Sdk.Entities;
using Genetec.Sdk;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genetec.Dap.CodeSamples;

public class TriggerAlarmSample : SampleBase
{
    protected override Task RunAsync(Engine engine, CancellationToken token)
    {
        // TODO: Replace the following GUIDs with the actual GUIDs from your system.

        Guid alarmGuid = new("alarm-guid-here"); // The GUID of the alarm to trigger. Replace with the GUID of the alarm you want to trigger.

        Guid sourceEntityGuid = new("source-entity-guid-here"); // Optional: The GUID of the entity that is the source of the alarm. Replace with the GUID of the entity that is the source of the alarm.

        DynamicAlarmContent alarmContent = new(context: "This is the context for the alarm"); // Optional: Create a new instance of DynamicAlarmContent to provide additional information for the alarm.
        alarmContent.Priority = 1; // Override the default priority of the alarm. 1 is the highest priority.
        alarmContent.AttachedEntities.Add(new Guid("entity-guid-here")); // Add any entities to this specific alarm instance.
        alarmContent.ForwardedRecipients.Add(new Guid("recipient-guid-here")); // Add any recipients to this specific alarm instance.
        alarmContent.Urls.Add("http://example.com"); // Add any URLs to this specific alarm instance.

        GeoCoordinate location = new(latitude: 1.3521, longitude: 103.8198); // Optional: The location of the alarm. Replace with the actual location of the alarm.

        int instanceId = engine.AlarmManager.TriggerAlarm(alarmGuid, sourceEntityGuid, alarmContent, location);

        Console.WriteLine(instanceId == -1 ? "Failed to trigger alarm." : $"Alarm triggered with instance ID: {instanceId}");

        return Task.CompletedTask;
    }
}