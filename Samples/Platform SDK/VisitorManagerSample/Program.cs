// Copyright 2024 Genetec
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.CustomFields;
using Genetec.Sdk.Queries;
using Sdk;
using Sdk.Entities.AccessControl.Visitors;

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
            const string visitorFirstName = "John";
            const string visitorLastName = "Doe";
            const string visitorMobileNumber = "123-456-7890";

            Visitor visitor;

            ArchivedVisitorData archivedVisitorData = await FindArchivedVisitor(engine, visitorFirstName, visitorLastName, visitorMobileNumber);
            if (archivedVisitorData is null)
            {
                visitor = await CreateVisitor();
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

            await Task.Delay(TimeSpan.FromSeconds(5));

            PerformCheckout(visitor);

            Task<Visitor> CreateVisitor()
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
        }
        else
        {
            Console.WriteLine($"Logon failed: {state}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void DisplayVisitorInfo(Visitor visitor)
    {
        Console.WriteLine("Visitor information:");
        Console.WriteLine($"First name: {visitor.FirstName}");
        Console.WriteLine($"Last name: {visitor.LastName}");
        Console.WriteLine($"Mobile number: {visitor.MobilePhoneNumber}");
        Console.WriteLine();
    }

    static void PerformCheckin(Visitor visitor)
    {
        Console.WriteLine("Check-in");
        visitor.Status.Activate();

        Console.WriteLine($"Check-in date: {visitor.CheckinDate.ToLocalTime()}");
        Console.WriteLine($"Arrival date: {visitor.Arrival.ToLocalTime()}");
        Console.WriteLine($"Visitor status: {visitor.Status.State}");
        Console.WriteLine();
    }

    static void PerformCheckout(Visitor visitor)
    {
        Console.WriteLine("Checkout");
        visitor.Checkout();

        Console.WriteLine();
    }

    static async Task<ArchivedVisitorData> FindArchivedVisitor(Engine engine, string firstName, string lastName, string mobileNumber)
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
            VisitorState = row.Field<VisitorState>("VisitorState"),
        }).LastOrDefault();
    }

    static void DisplayArchivedVisitorInfo(ArchivedVisitor archivedVisitor)
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