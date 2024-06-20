// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

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
            // Set the server URL, username, and password for authentication
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            // Create an instance of the Engine class
            using var engine = new Engine();

            // Subscribe to the EventReceived event to handle access events
            engine.EventReceived += (sender, e) =>
            {
                // Check if the received event is an AccessEvent
                if (e.Event is AccessEvent accessEvent)
                {
                    // Get the AccessPoint and Reader entities associated with the access event
                    if (engine.GetEntity(accessEvent.AccessPoint) is AccessPoint accessPoint && engine.GetEntity(accessPoint.Device) is Reader reader)
                    {
                        // Iterate over the credential formats associated with the access event
                        foreach (CredentialFormat format in GetCredentialFormats(accessEvent))
                        {
                            // Print the card swipe information
                            Console.WriteLine($"Card swiped ({format.RawData}) on {reader.Name}");
                        }
                    }
                }
            };

            // Attempt to log on to the server with the provided credentials
            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            // Check if the logon was successful
            if (state == ConnectionStateCode.Success)
            {
                // Load the access points if logon was successful
                await LoadAccessPoints();
            }
            else
            {
                // Print an error message if logon failed
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            // Method to load access points
            async Task LoadAccessPoints()
            {
                // Create an EntityConfigurationQuery for access points
                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.Add(EntityType.AccessPoint);
                query.DownloadAllRelatedData = true;
                await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            }

            // Method to get credential formats from an AccessEvent
            IEnumerable<CredentialFormat> GetCredentialFormats(AccessEvent accessEvent)
            {
                // If the event is an AccessPointCredentialUnknownEvent, deserialize the XmlCredential
                if (accessEvent is AccessPointCredentialUnknownEvent unknownEvent)
                {
                    yield return CredentialFormat.Deserialize(unknownEvent.XmlCredential);
                }
                else
                {
                    // Otherwise, get the credential formats from the Credentials collection
                    IEnumerable<CredentialFormat> formats = accessEvent.Credentials.Select(engine.GetEntity).OfType<Credential>().Select(credential => credential.Format);
                    foreach (CredentialFormat format in formats)
                    {
                        yield return format;
                    }
                }
            }
        }
    }
}
