// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Events.AccessPoint;
using Genetec.Sdk.Workflows;

namespace Genetec.Dap.CodeSamples;

public class RaiseEventSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Retrieve the credential and reader entities using their GUIDs
        var credential = (Credential)engine.GetEntity(new Guid("YOUR_CREDENTIAL_GUID")); // TODO: Replace with your actual credential GUID
        var reader = (Reader)engine.GetEntity(new Guid("YOUR_READER_GUID")); // TODO: Replace with your actual reader GUID

        RaiseAccessGranted(engine, credential, reader);

        await Task.CompletedTask;
    }

    private void RaiseAccessGranted(Engine engine, Credential credential, Reader reader)
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