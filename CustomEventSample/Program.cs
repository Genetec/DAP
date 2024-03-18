namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Entities.CustomEvents;
    using Sdk.Events;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            var engine = new Engine();
            await engine.LogOnAsync(server, username, password);

            var config = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);

            const int customEventId = 1000;

            CustomEvent customEvent = config.CustomEventService.CustomEvents.FirstOrDefault(c => c.Id == customEventId);

            if (customEvent is null)
            {
                customEvent = config.CustomEventService.CreateCustomEventBuilder()
                    .SetEntityType(EntityType.Camera)
                    .SetId(customEventId)
                    .SetName("Custom event")
                    .Build();

                await config.CustomEventService.AddAsync(customEvent);
            }

            Entity source = engine.GetEntity(EntityType.Camera, 1);

            var customEventInstance = (CustomEventInstance)engine.ActionManager.BuildEvent(EventType.CustomEvent, source.Guid);
            customEventInstance.Id = new CustomEventId(customEventId);
            customEventInstance.Message = "Custom event Message";
            customEventInstance.ExtraHiddenPayload = "This is a hidden payload";
          
            engine.ActionManager.RaiseEvent(customEventInstance);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}