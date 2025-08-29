// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.AccessControl.Visitors;
using Genetec.Sdk.Entities.CustomFields;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

public class VisitorManagerSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        const string visitorFirstName = "John";
        const string visitorLastName = "Doe";
        const string visitorMobileNumber = "123-456-7890";

        Visitor visitor;

        ArchivedVisitorData archivedVisitorData = await FindArchivedVisitor(engine, visitorFirstName, visitorLastName, visitorMobileNumber);
        if (archivedVisitorData is null)
        {
            visitor = await CreateVisitor(engine, visitorFirstName, visitorLastName, visitorMobileNumber);
        }
        else
        {
            ArchivedVisitor archivedVisitor = engine.VisitorManager.GetArchivedVisitor(archivedVisitorData.Guid);

            DisplayArchivedVisitorInfo(archivedVisitor);

            Console.WriteLine("Creating visitor from archived visitor");
            visitor = engine.VisitorManager.CreateVisitor(archivedVisitor);
        }

        DisplayVisitorInfo(visitor);

        PerformCheckin(visitor);

        Console.WriteLine("Waiting for 5 seconds before checkout...");
        await Task.Delay(TimeSpan.FromSeconds(5), token);

        PerformCheckout(visitor);
    }

    Task<Visitor> CreateVisitor(Engine engine, string visitorFirstName, string visitorLastName, string visitorMobileNumber)
    {
        Console.WriteLine("Creating visitor");

        return engine.TransactionManager.ExecuteTransactionAsync(() =>
        {
            var visitor = (Visitor)engine.CreateEntity($"{visitorFirstName} {visitorLastName}", EntityType.Visitor);
            visitor.FirstName = visitorFirstName;
            visitor.LastName = visitorLastName;
            visitor.MobilePhoneNumber = visitorMobileNumber;
            return visitor;
        });
    }

    void DisplayVisitorInfo(Visitor visitor)
    {
        Console.WriteLine("Visitor information:");
        Console.WriteLine($"First name: {visitor.FirstName}");
        Console.WriteLine($"Last name: {visitor.LastName}");
        Console.WriteLine($"Mobile number: {visitor.MobilePhoneNumber}");
        Console.WriteLine();
    }

    void PerformCheckin(Visitor visitor)
    {
        Console.WriteLine("Check-in");
        visitor.Status.Activate();

        Console.WriteLine($"Check-in date: {visitor.CheckinDate.ToLocalTime()}");
        Console.WriteLine($"Arrival date: {visitor.Arrival.ToLocalTime()}");
        Console.WriteLine($"Visitor status: {visitor.Status.State}");
        Console.WriteLine();
    }

    void PerformCheckout(Visitor visitor)
    {
        Console.WriteLine("Checkout");
        visitor.Checkout();

        Console.WriteLine();
    }

    async Task<ArchivedVisitorData> FindArchivedVisitor(Engine engine, string firstName, string lastName, string mobileNumber)
    {
        Console.WriteLine("Searching archived visitor");

        var query = (VisitorQuery)engine.ReportManager.CreateReportQuery(ReportType.Visitor);
        query.FirstName = firstName;
        query.FirstNameSearchMode = StringSearchMode.Is;
        query.LastName = lastName;
        query.LastNameSearchMode = StringSearchMode.Is;
        query.MobilePhoneNumber = mobileNumber;
        query.MobilePhoneNumberSearchMode = StringSearchMode.Is;
        query.LastConfiguration = true;

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

        return args.Data.AsEnumerable().Select(row => new ArchivedVisitorData
        {
            Guid = row.Field<Guid>("Guid"),
            FirstName = row.Field<string>("FirstName"),
            LastName = row.Field<string>("LastName"),
            EmailAddress = row.Field<string>("Email"),
            CustomFields = row.Field<CustomFieldsCollection>("CustomField"),
            ActivationDate = row.Field<DateTime>("ActivationDate"),
            ExpirationDate = row.Field<DateTime>("ExpirationDate"),
            Picture = row.Field<Guid>("Picture"),
            Thumbnail = row.Field<Guid>("Thumbnail"),
            CheckinDate = row.Field<DateTime>("CheckinDate"),
            CheckoutDate = row.Field<DateTime>("CheckoutDate"),
            Description = row.Field<string>("Description"),
            Escort = row.Field<Guid>("Escort"),
            Escort2 = row.Field<Guid>("Escort2"),
            MandatoryEscort = row.Field<bool>("MandatoryEscort"),
            VisitDate = row.Field<DateTime>("VisitDate"),
            MobilePhoneNumber = row.Field<string>("MobilePhoneNumber"),
            VisitorState = row.Field<VisitorState>("VisitorState")
        }).LastOrDefault();
    }

    void DisplayArchivedVisitorInfo(ArchivedVisitor archivedVisitor)
    {
        Console.WriteLine("Archived visitor information:");
        Console.WriteLine($"Name: {archivedVisitor.Name}");
        Console.WriteLine($"First name: {archivedVisitor.FirstName}");
        Console.WriteLine($"Last name: {archivedVisitor.LastName}");
        Console.WriteLine($"Email: {archivedVisitor.EmailAddress}");
        Console.WriteLine($"Mobile number: {archivedVisitor.MobilePhoneNumber}");
        Console.WriteLine($"Activation date: {archivedVisitor.ActivationDate}");
        Console.WriteLine($"Expiration date: {archivedVisitor.ExpirationDate}");
        Console.WriteLine($"Check-in date: {archivedVisitor.CheckinDate.ToLocalTime()}");
        Console.WriteLine($"Checkout date: {archivedVisitor.CheckoutDate.ToLocalTime()}");
        Console.WriteLine();
    }
}