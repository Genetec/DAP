using Genetec.Sdk.Entities;
using Genetec.Sdk;
using System;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";

    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state == ConnectionStateCode.Success)
    {
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
    }
    else
    {
        Console.WriteLine($"Logon failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

