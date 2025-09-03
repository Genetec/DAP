// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Credentials;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

public class CardholderSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        List<Cardholder> cardholders = await CreateCardholdersWithCredentials(engine);

        DisplayCardholders(engine, cardholders);

        await DeleteCardholders(engine, cardholders);
    }

    private async Task<List<Cardholder>> CreateCardholdersWithCredentials(Engine engine)
    {
        Console.WriteLine("Creating cardholders if they don't exist...");

        // Check for existing cardholders first
        Cardholder cardholder1 = (await FindCardholders(engine, "Michael", "Johnson", "michael.johnson@techcorp.com", "+1555123456")).FirstOrDefault();
        Cardholder cardholder2 = (await FindCardholders(engine, "Sarah", "Williams", "sarah.williams@healthsystems.org", "+1555987654")).FirstOrDefault();

        // Credential formats for the cardholders
        CredentialFormat format1 = new WiegandStandardCredentialFormat(facility: 1, cardId: 1001);
        CredentialFormat format2 = new WiegandStandardCredentialFormat(facility: 1, cardId: 1002);

        // Try to find existing credentials with the same formats to avoid duplicates
        Credential credential1 = await FindCredential(engine, format1);
        Credential credential2 = await FindCredential(engine, format2);

        return await engine.TransactionManager.ExecuteTransactionAsync(() =>
        {
            var cardholders = new List<Cardholder>();

            if (cardholder1 == null)
            {
                cardholder1 = (Cardholder)engine.CreateEntity("Michael Johnson", EntityType.Cardholder);
                cardholder1.FirstName = "Michael";
                cardholder1.LastName = "Johnson";
                cardholder1.EmailAddress = "michael.johnson@techcorp.com";
                cardholder1.MobilePhoneNumber = "+1555123456";
                cardholder1.Status.Activate(DateTime.UtcNow.AddDays(3)); // Activate in 3 days
                cardholder1.Status.ExpireWhenNotUsedInDays(30); // Expires 30 days after last use
            }
            cardholders.Add(cardholder1);

            if (credential1 is null) // Create new credential if not found
            {
                credential1 = engine.EntityManager.GetCredentialBuilder()
                    .SetName($"Card - {cardholder1.Name}")
                    .SetFormat(format1)
                    .Build();

                Console.WriteLine($"Created credential: {credential1.Name} (Format: {format1.Name}, Raw Data: {format1.RawData})");
            }
            else if (credential1.CardholderGuid != Guid.Empty) // Credential already assigned to a cardholder
            {
                credential1.CardholderGuid = Guid.Empty; // Unassign from previous cardholder if assigned
            }
            cardholder1.Credentials.Add(credential1.Guid); // Assign credential to cardholder

            if (cardholder2 == null)
            {
                cardholder2 = (Cardholder)engine.CreateEntity("Sarah Williams", EntityType.Cardholder);
                cardholder2.FirstName = "Sarah";
                cardholder2.LastName = "Williams";
                cardholder2.EmailAddress = "sarah.williams@healthsystems.org";
                cardholder2.MobilePhoneNumber = "+1555987654";
                cardholder2.Status.Activate(DateTime.UtcNow.AddDays(1)); // Activate in 1 day
                cardholder2.Status.ExpireOnFirstUseInDays(3); // Expires 3 days after first use
            }
            cardholders.Add(cardholder2);

            if (credential2 is null) // Create new credential if not found
            {
                credential2 = engine.EntityManager.GetCredentialBuilder()
                    .SetName($"Card - {cardholder2.Name}")
                    .SetFormat(format2)
                    .Build();

                Console.WriteLine($"Created credential: {credential2.Name} (Format: {format2.Name}, Raw Data: {format2.RawData})");
            }
            else if (credential2.CardholderGuid != Guid.Empty) // Credential already assigned to a cardholder
            {
                credential2.CardholderGuid = Guid.Empty; // Unassign from previous cardholder if assigned
            }
            cardholder2.Credentials.Add(credential2.Guid); // Assign credential to cardholder

            return cardholders;
        });
    }

    private Task DeleteCardholders(Engine engine, List<Cardholder> cardholders)
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

    private void DisplayCardholders(Engine engine, ICollection<Cardholder> cardholders)
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
            Console.WriteLine($"   GUID: {cardholder.Guid}");
            Console.WriteLine($"   First Name: {cardholder.FirstName ?? "N/A"}");
            Console.WriteLine($"   Last Name: {cardholder.LastName ?? "N/A"}");
            Console.WriteLine($"   Email: {cardholder.EmailAddress ?? "N/A"}");
            Console.WriteLine($"   Mobile Phone: {cardholder.MobilePhoneNumber ?? "N/A"}");
            Console.WriteLine($"   Created: {cardholder.CreatedOn:f}");
            Console.WriteLine($"   State: {cardholder.Status.State}");
            Console.WriteLine($"   Activation Date: {cardholder.Status.ActivationDate}");
            Console.WriteLine($"   Expiration Date: {cardholder.Status.ExpirationDate}");
            Console.WriteLine();

            if (cardholder.Credentials.Any())
            {
                Console.WriteLine($"   Credentials for {cardholder.Name}:");
                foreach (var credential in cardholder.Credentials.Select(engine.GetEntity).OfType<Credential>())
                {
                    CredentialFormat format = credential.Format;
                    Console.WriteLine($"     • {credential.Name} (Format: {format.Name})");
                    Console.WriteLine($"       GUID: {credential.Guid}");
                    Console.WriteLine($"       Raw Data: {format.RawData}");
                    Console.WriteLine($"       State: {credential.Status.State}");
                    Console.WriteLine($"       Activation Date: {credential.Status.ActivationDate}");
                    Console.WriteLine($"       Expiration Date: {credential.Status.ExpirationDate}");
                }
            }
            else
            {
                Console.WriteLine($"   No credentials assigned to {cardholder.Name}");
            }

            Console.WriteLine();
        }
    }

    private async Task<Credential> FindCredential(Engine engine, CredentialFormat format)
    {
        var query = (CredentialConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.CredentialConfiguration);
        query.UniqueIds.Add(format);
        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        return args.Data.AsEnumerable().Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid)))).OfType<Credential>().FirstOrDefault();
    }

    private async Task<IList<Cardholder>> FindCardholders(Engine engine, string firstName, string lastName, string email, string mobilePhoneNumber)
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