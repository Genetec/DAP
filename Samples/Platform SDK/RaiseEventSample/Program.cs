// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Events.AccessPoint;
    using Sdk.Workflows;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var engine = new Engine();

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                // Retrieve the credential and reader entities using their GUIDs
                var credential = (Credential)engine.GetEntity(new Guid("YOUR_CREDENTIAL_GUID"));
                var reader = (Reader)engine.GetEntity(new Guid("YOUR_READER_GUID"));

                RaiseAccessGranted(engine, credential, reader);
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void RaiseAccessGranted(Engine engine, Credential credential, Reader reader)
        {
            using EventTransaction transaction = engine.TransactionManager.CreateEventTransaction();

            // Find the access point
            AccessPoint accessPoint = reader.AccessPoint.Select(engine.GetEntity).OfType<AccessPoint>().FirstOrDefault(point => point.AccessPointType == AccessPointType.CardReader);

            if (accessPoint != null)
            {
                var accessEvent = (AccessEvent)engine.ActionManager.BuildEvent(EventType.AccessGranted, accessPoint.Guid);
                accessEvent.Cardholder = credential.CardholderGuid;
                accessEvent.Credentials.Add(credential.Guid);
                accessEvent.DoorSideGuid = accessPoint.Guid;

                // Add the event to the transaction
                transaction.AddEvent(accessEvent);

                Console.WriteLine("Access granted event raised successfully.");
            }
            else
            {
                Console.WriteLine("No card reader access point found.");
            }
        }
    }
}