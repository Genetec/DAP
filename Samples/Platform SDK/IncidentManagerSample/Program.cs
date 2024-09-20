// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Sdk.Queries;
using Genetec.Sdk.Workflows;
using Genetec.Sdk;

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
            Console.WriteLine("\nExisting Incidents:");

            var query = (IncidentSdkQuery)engine.ReportManager.CreateReportQuery(ReportType.Incident);
            QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            IEnumerable<IncidentData> incidentData = args.Data.AsEnumerable().Select(CreateIncidentData);

            foreach (var data in incidentData)
            {
                PrintIncidentDetails(data);
            }

            IncidentManager incidentManager = engine.IncidentManager;

            const string newCategory = "NewIncidentCategory";
            Console.WriteLine($"Creating new incident category: {newCategory}");
            bool categoryCreated = incidentManager.AddIncidentCategory(newCategory);
            Console.WriteLine($"{(categoryCreated ? "✓" : "✗")} Incident category '{newCategory}' created");

            Console.WriteLine("\nListing all incident categories:");
            foreach (string category in incidentManager.GetIncidentCategories())
            {
                Console.WriteLine($"- {category}");
            }

            Console.WriteLine("\nCreating a new incident...");
            Incident newIncident = incidentManager.CreateIncident();

            newIncident.Title = "Sample Incident";
            newIncident.Category = newCategory;
            newIncident.AlarmInstance = 1234;
            newIncident.Timestamp = DateTime.Now;
            newIncident.Location = new GeoCoordinate(45.4215, -75.6972); // Example coordinates
            newIncident.Notes = "This is a sample incident note.";

            Console.WriteLine("Saving the new incident...");
            incidentManager.SaveIncident(newIncident);
            Console.WriteLine("Incident saved successfully");

            Incident retrievedIncident = incidentManager.GetIncident(newIncident.Guid);

            Console.WriteLine("\nIncident Details:");
            Console.WriteLine($"Title: {retrievedIncident.Title}");
            Console.WriteLine($"Category: {retrievedIncident.Category}");
            Console.WriteLine($"Created By: {retrievedIncident.CreatedBy}");
            Console.WriteLine($"Creation Time: {retrievedIncident.CreationTime}");
            Console.WriteLine($"Location: {retrievedIncident.Location?.Latitude}, {retrievedIncident.Location?.Longitude}");
            Console.WriteLine($"Notes: {retrievedIncident.Notes}");

            Console.WriteLine("\nDeleting the incident...");
            bool deleteSuccess = incidentManager.DeleteIncident(newIncident.Guid);
            Console.WriteLine($"{(deleteSuccess ? "✓" : "✗")} Incident deleted");

            Console.WriteLine($"\nDeleting incident category '{newCategory}'...");
            bool categoryDeleted = incidentManager.RemoveIncidentCategory(newCategory);
            Console.WriteLine($"{(categoryDeleted ? "✓" : "✗")} Incident category '{newCategory}' deleted");
        }
        else
        {
            Console.WriteLine($"Logon failed: {state}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();

        IncidentData CreateIncidentData(DataRow row) => new IncidentData
        {
            InstanceGuid = row.Field<Guid>(IncidentSdkQuery.InstanceGuidColumnName),
            Latitude = row.Field<double>(IncidentSdkQuery.LatitudeColumnName),
            Longitude = row.Field<double>(IncidentSdkQuery.LongitudeColumnName),
            Title = row.Field<string>(IncidentSdkQuery.TitleColumnName),
            Notes = row.Field<string>(IncidentSdkQuery.NoteColumnName),
            AlarmInstance = row.Field<int>(IncidentSdkQuery.AlarmInstanceColumnName),
            Category = row.Field<string>(IncidentSdkQuery.CategoryColumnName),
            CreatedBy = row.Field<Guid>(IncidentSdkQuery.CreatedByColumnName),
            CreationTime = row.Field<DateTime>(IncidentSdkQuery.CreationTimestampColumnName),
            Data = row.Field<string>(IncidentSdkQuery.DataColumnName),
            AttachedData = row.Field<string>(IncidentSdkQuery.AttachedDataColumnName),
            Timestamp = row.Field<DateTime>(IncidentSdkQuery.IncidentTimestamp),
            Event = row.Field<EventType>(IncidentSdkQuery.EventTypeColumnName),
            LastModifiedBy = row.Field<Guid>(IncidentSdkQuery.ModifiedByColumnName),
            References = string.IsNullOrEmpty(row.Field<string>(IncidentSdkQuery.GuidReferenceColumnName)) ? [] : new Collection<Guid>(row.Field<string>(IncidentSdkQuery.GuidReferenceColumnName).Split(',').Select(Guid.Parse).ToList())
        };
    }

    static void PrintIncidentDetails(IncidentData data)
    {
        Console.WriteLine("\nIncident Details:");
        Console.WriteLine($"Title: {data.Title}");
        Console.WriteLine($"Category: {data.Category}");
        Console.WriteLine($"Created By: {data.CreatedBy}");
        Console.WriteLine($"Creation Time: {data.CreationTime}");
        Console.WriteLine($"Location: {data.Latitude}, {data.Longitude}");
        Console.WriteLine($"Notes: {data.Notes}");
        Console.WriteLine($"Alarm Instance: {data.AlarmInstance}");
        Console.WriteLine($"Event: {data.Event}");
        Console.WriteLine($"Last Modified By: {data.LastModifiedBy}");
        Console.WriteLine($"Timestamp: {data.Timestamp}");
        Console.WriteLine($"Data: {data.Data}");
        Console.WriteLine($"Attached Data: {data.AttachedData}");
        Console.WriteLine("References:");

        foreach (var reference in data.References)
        {
            Console.WriteLine($"- {reference}");
        }
    }
}