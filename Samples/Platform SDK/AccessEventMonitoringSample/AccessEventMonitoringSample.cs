// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sdk;
using Sdk.Credentials;
using Sdk.Entities;
using Sdk.Events.AccessPoint;

class AccessEventMonitoringSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        engine.EventReceived += OnEventReceived;

        await LoadEntities(engine, token, EntityType.AccessPoint); // Load all access points into the entity cache. This is necessary to receive events.

        Console.WriteLine("Monitoring access events... Press Ctrl+C to stop.");

        // Wait indefinitely until cancellation is requested
        await Task.Delay(Timeout.Infinite, token);
    }

    void OnEventReceived(object sender, EventReceivedEventArgs e)
    {
        var engine = (Engine)sender;
        if (e.Event is AccessEvent accessEvent)
        {
            if (engine.GetEntity(accessEvent.AccessPoint) is AccessPoint accessPoint && engine.GetEntity(accessPoint.Device) is Device device)
            {
                var accessPointGroup = (AccessPointGroup)engine.GetEntity(accessPoint.AccessPointGroup);
                var cardholder = engine.GetEntity(accessEvent.Cardholder);

                foreach (CredentialFormat format in GetCredentialFormats(accessEvent))
                {
                    Console.WriteLine(
                        $"[{e.Timestamp:yyyy-MM-dd HH:mm:ss}] Access Event: {e.EventType}\n" +
                        $"  Cardholder:  {cardholder?.Name ?? "Unknown"}\n" +
                        $"  Credential Format: {format}\n" +
                        $"  Access Point Group: {accessPointGroup.Name}\n" +
                        $"  Access Point: {accessPoint.Name}\n" +
                        $"  Device:      {device.Name}\n"
                    );
                }
            }
        }

        IEnumerable<CredentialFormat> GetCredentialFormats(AccessEvent accessEvent)
        {
            if (accessEvent is AccessPointCredentialUnknownEvent unknownEvent)
            {
                yield return CredentialFormat.Deserialize(unknownEvent.XmlCredential);
            }

            IEnumerable<CredentialFormat> formats = accessEvent.Credentials.Select(engine.GetEntity).OfType<Credential>().Select(credential => credential.Format);

            foreach (var format in formats)
            {
                yield return format;
            }
        }
    }


}