// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

public class CardholderSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        List<Cardholder> newCardholders = await CreateCardholders(engine);

        IList<Cardholder> cardholders = await FindCardholders(engine, "Michael", "Johnson", "michael.johnson@techcorp.com", "+1555123456");

        Cardholder cardholder = cardholders.FirstOrDefault();
        if (cardholder != null)
        {
            await UpdateCardholder(engine, cardholder);
        }

        await LoadEntities(engine, token, EntityType.Cardholder);
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

            cardholder1.Status.Activate(DateTime.UtcNow.AddDays(3)); // Activate the cardholder in 3 days
            cardholder1.Status.ExpireWhenNotUsedInDays(30); // Set the cardholder to expire if not used in 30 days

            cardholders.Add(cardholder1);

            var cardholder2 = (Cardholder)engine.CreateEntity("Sarah Williams", EntityType.Cardholder);
            cardholder2.FirstName = "Sarah";
            cardholder2.LastName = "Williams";
            cardholder2.EmailAddress = "sarah.williams@healthsystems.org";
            cardholder2.MobilePhoneNumber = "+1555987654";
            cardholders.Add(cardholder2);

            cardholder1.Status.Activate(DateTime.UtcNow.AddDays(1)); // Activate the cardholder in 1 day
            cardholder1.Status.ExpireOnFirstUseInDays(3); // Set the cardholder to expire 3 days after first use

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
}