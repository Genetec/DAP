// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

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
        var credential = (Credential)engine.GetEntity(new Guid("YOUR_CREDENTIAL_GUID"));
        var reader = (Reader)engine.GetEntity(new Guid("YOUR_READER_GUID"));

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