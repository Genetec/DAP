// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk.Events.AccessPoint;
    using Genetec.Sdk;
    using Genetec.Sdk.Credentials;
    using Genetec.Sdk.Entities;
    using Genetec.Sdk.Queries;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var engine = new Engine();
            engine.EventReceived += OnEventReceived;

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                await LoadAccessPoints();
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            Task LoadAccessPoints()
            {
                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.Add(EntityType.AccessPoint);
                query.DownloadAllRelatedData = true;
                return Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
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

            void OnEventReceived(object sender, EventReceivedEventArgs e)
            {
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
            }
        }
    }
}
