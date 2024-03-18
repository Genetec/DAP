// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Events.AccessPoint;

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

            var door = (Door)engine.GetEntity(EntityType.Door, 1);
            var reader = (Reader)engine.GetEntity(door.DoorSideIn.Reader.Device);

            RaiseAccessGranted(engine, (Credential)engine.GetEntity(new Guid("0083db96-b7bf-4fc8-9534-12239bcc1b92")), reader);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }


       static void RaiseAccessGranted(Engine engine, Credential credential, Reader reader)
       {
           using var transaction = engine.TransactionManager.CreateEventTransaction();

           Guid groupId = Guid.NewGuid();
           foreach (var accessPoint in reader.AccessPoint.Select(engine.GetEntity).OfType<AccessPoint>())
           {
               var accessEvent = (AccessEvent)engine.ActionManager.BuildEvent(EventType.AccessGranted, accessPoint.Guid);
               accessEvent.Cardholder = credential.CardholderGuid;
               accessEvent.Credentials.Add(credential.Guid);
               accessEvent.GroupId = groupId;

               if (accessPoint.AccessPointType == AccessPointType.CardReader)
               {
                   accessEvent.DoorSideGuid = accessPoint.Guid;
               }

               transaction.AddEvent(accessEvent);
           }
       }
    }
}
