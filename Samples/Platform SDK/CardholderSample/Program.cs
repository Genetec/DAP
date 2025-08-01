// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

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
            List<Cardholder> newCardholders = await CreateCardholders(engine);

            IList<Cardholder> cardholders = await FindCardholders(engine, "Michael", "Johnson", "michael.johnson@techcorp.com", "+1555123456");

            Cardholder cardholder = cardholders.FirstOrDefault();
            if (cardholder != null)
            {
                await UpdateCardholder(engine, cardholder);
            }

            await LoadCardholders(engine);
            cardholders = engine.GetEntities(EntityType.Cardholder).OfType<Cardholder>().Take(10).ToList();

            DisplayCardholders(cardholders);

            await DeleteCardholders(engine, newCardholders);
        }
        else
        {
            Console.WriteLine($"Logon failed: {state}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey(true);
    }

    static Task<List<Cardholder>> CreateCardholders(Engine engine)
    {
        Console.WriteLine("Creating multiple cardholders in a single transaction...");

        return engine.TransactionManager.ExecuteTransactionAsync(() =>
        {
            var cardholders = new List<Cardholder>();

            var cardholder1 = (Cardholder)engine.CreateEntity("Michael Johnson", EntityType.Cardholder);
            cardholder1.FirstName = "Michael";
            cardholder1.LastName = "Johnson";
            cardholder1.EmailAddress = "michael.johnson@techcorp.com";
            cardholder1.MobilePhoneNumber = "+1555123456";
            cardholders.Add(cardholder1);

            var cardholder2 = (Cardholder)engine.CreateEntity("Sarah Williams", EntityType.Cardholder);
            cardholder2.FirstName = "Sarah";
            cardholder2.LastName = "Williams";
            cardholder2.EmailAddress = "sarah.williams@healthsystems.org";
            cardholder2.MobilePhoneNumber = "+1555987654";
            cardholders.Add(cardholder2);

            Console.WriteLine($"Created {cardholders.Count} cardholders successfully");
            return cardholders;
        });
    }

    static Task UpdateCardholder(Engine engine, Cardholder cardholder)
    {
        Console.WriteLine($"Updating cardholder: {cardholder.Name}");

        return engine.TransactionManager.ExecuteTransactionAsync(() =>
        {
            cardholder.EmailAddress = "michael.johnson.updated@techcorp.com";
            cardholder.MobilePhoneNumber = "+1555111222";
        });
    }

    static Task DeleteCardholders(Engine engine, List<Cardholder> cardholders)
    {
        Console.WriteLine($"Deleting {cardholders.Count} cardholders...");

        return engine.TransactionManager.ExecuteTransactionAsync(() =>
        {
            foreach (var cardholder in cardholders)
            {
                Console.WriteLine($"Deleting cardholder: {cardholder.Name}");
                engine.DeleteEntity(cardholder.Guid);
            }
        });
    }

    static async Task LoadCardholders(Engine engine)
    {
        Console.WriteLine("Loading cardholders...");

        var query = (CardholderConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.CardholderConfiguration);
        query.Page = 1;
        query.PageSize = 1000;

        int total = 0;
        QueryCompletedEventArgs args;
        do
        {
            args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            query.Page++;

            total += Math.Min(args.Data.Rows.Count, query.PageSize);

            Console.Write($"\rLoaded page {query.Page} (Total: {total})");

        } while (args.Data.Rows.Count >= query.PageSize);

        int count = engine.GetEntities(EntityType.Cardholder).Count;

        Console.WriteLine($"\n{count} cardholders loaded into the entity cache");
    }

    static void DisplayCardholders(ICollection<Cardholder> cardholders)
    {
        if (!cardholders.Any())
        {
            Console.WriteLine("No cardholders found in the system.");
            return;
        }

        Console.WriteLine($"\nDisplaying {cardholders.Count} cardholders:");
        foreach (Cardholder cardholder in cardholders)
        {
            DisplayCardholderInfo(cardholder);
        }

        Console.WriteLine();
    }

    static async Task<IList<Cardholder>> FindCardholders(Engine engine, string firstName, string lastName, string email, string mobilePhoneNumber)
    {
        Console.WriteLine($"Searching for cardholders with specified criteria...");

        var query = (CardholderConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.CardholderConfiguration);

        if (!string.IsNullOrWhiteSpace(firstName))
        {
            query.FirstName = firstName;
            query.FirstNameSearchMode = StringSearchMode.Is;
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            query.LastName = lastName;
            query.LastNameSearchMode = StringSearchMode.Is;
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            query.Email = email;
            query.EmailSearchMode = StringSearchMode.Is;
        }

        if (!string.IsNullOrWhiteSpace(mobilePhoneNumber))
        {
            query.MobilePhoneNumber = mobilePhoneNumber;
            query.MobilePhoneNumberSearchMode = StringSearchMode.Is;
        }

        QueryCompletedEventArgs results = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        var foundCardholders = results.Data.AsEnumerable().Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid)))).OfType<Cardholder>().ToList();

        Console.WriteLine($"Found {foundCardholders.Count} matching cardholder(s)");
        return foundCardholders;
    }

    static void DisplayCardholderInfo(Cardholder cardholder)
    {
        Console.WriteLine($"   GUID: {cardholder.Guid}");
        Console.WriteLine($"    Name: {cardholder.Name}");
        Console.WriteLine($"    First Name: {cardholder.FirstName ?? "N/A"}");
        Console.WriteLine($"    Last Name: {cardholder.LastName ?? "N/A"}");
        Console.WriteLine($"    Email: {cardholder.EmailAddress ?? "N/A"}");
        Console.WriteLine($"    Mobile Phone: {cardholder.MobilePhoneNumber ?? "N/A"}");
        Console.WriteLine($"    Created: {cardholder.CreatedOn:f}");
        Console.WriteLine();
    }
}