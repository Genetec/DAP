// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

SdkResolver.Initialize();

await RunSample();

Console.WriteLine("Press any key to exit...");
Console.ReadKey(true);

async Task RunSample()
{
    // Connection parameters for your Security Center server
    const string server = "localhost";  // Specify the IP address or hostname of your Security Center server.
    const string username = "admin";    // Enter the username for Security Center authentication.
    const string password = "";         // Provide the corresponding password for the specified username.

    using var engine = new Engine();

    engine.LoginManager.LogonStatusChanged += (sender, args) => Console.WriteLine($"Logon status: {args.Status}");
    engine.LoginManager.LogonFailed += (sender, e) => Console.WriteLine($"Logon Failed | Error Message: {e.FormattedErrorMessage} | Error Code: {e.FailureCode}");

    // Set up cancellation support
    using var cancellationTokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        Console.WriteLine("Cancelling...");
        e.Cancel = true;
        cancellationTokenSource.Cancel();
    };

    Console.WriteLine($"Logging to {server}... Press Ctrl+C to cancel");

    ConnectionStateCode state = await engine.LoginManager.LogOnAsync(server, username, password, cancellationTokenSource.Token);
    if (state != ConnectionStateCode.Success)
    {
        Console.WriteLine($"logon failed: {state}");
        return;
    }

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

Task<List<Cardholder>> CreateCardholders(Engine engine)
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

Task UpdateCardholder(Engine engine, Cardholder cardholder)
{
    Console.WriteLine($"Updating cardholder: {cardholder.Name}");

    return engine.TransactionManager.ExecuteTransactionAsync(() =>
    {
        cardholder.EmailAddress = "michael.johnson.updated@techcorp.com";
        cardholder.MobilePhoneNumber = "+1555111222";
    });
}

Task DeleteCardholders(Engine engine, List<Cardholder> cardholders)
{
    Console.WriteLine($"Deleting {cardholders.Count} cardholders...");

    return engine.TransactionManager.ExecuteTransactionAsync(() =>
    {
        foreach (Cardholder cardholder in cardholders)
        {
            Console.WriteLine($"Deleting cardholder: {cardholder.Name}");
            engine.DeleteEntity(cardholder.Guid);
        }
    });
}

async Task LoadCardholders(Engine engine)
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

void DisplayCardholders(ICollection<Cardholder> cardholders)
{
    Console.WriteLine($"\nDisplaying {cardholders.Count} cardholders:");
    foreach (Cardholder cardholder in cardholders)
    {
        DisplayCardholderInfo(cardholder);
    }

    Console.WriteLine();

    void DisplayCardholderInfo(Cardholder cardholder)
    {
        Console.WriteLine($"   Name: {cardholder.Name}");
        Console.WriteLine($"    GUID: {cardholder.Guid}");
        Console.WriteLine($"    First Name: {cardholder.FirstName ?? "N/A"}");
        Console.WriteLine($"    Last Name: {cardholder.LastName ?? "N/A"}");
        Console.WriteLine($"    Email: {cardholder.EmailAddress ?? "N/A"}");
        Console.WriteLine($"    Mobile Phone: {cardholder.MobilePhoneNumber ?? "N/A"}");
        Console.WriteLine($"    Created: {cardholder.CreatedOn:f}");
        Console.WriteLine();
    }
}

async Task<IList<Cardholder>> FindCardholders(Engine engine, string firstName, string lastName, string email, string mobilePhoneNumber)
{
    Console.WriteLine("Searching for cardholders with specified criteria...");

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
    List<Cardholder> cardholders = results.Data.AsEnumerable().Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid)))).OfType<Cardholder>().ToList();

    Console.WriteLine($"Found {cardholders.Count} matching cardholder(s)");
    return cardholders;
}