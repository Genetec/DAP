// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk;
    using Genetec.Sdk.AccessControl.Credentials.CardCredentials;
    using Genetec.Sdk.Credentials;
    using Genetec.Sdk.Entities;
    using Genetec.Sdk.Queries;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            var engine = new Engine();
            engine.LoginManager.ConnectionRetry = -1;
            engine.LoginManager.LogonStatusChanged += (sender, args) => Console.WriteLine($"Logon status: {args.Status}");
            engine.LoginManager.LogonFailed += (sender, args) => Console.WriteLine($"Logon failed: {args.FailureCode}");

            ConnectionStateCode state = await engine.LogOnAsync(Properties.Settings.Default.Server, Properties.Settings.Default.Username, Properties.Settings.Default.Password);
            if (state == ConnectionStateCode.Success)
            {
                await LoadEntities(EntityType.Credential, EntityType.Cardholder);

                var cardholders = engine.GetEntities(EntityType.Cardholder)
                  .OfType<Cardholder>();
                foreach (var cardholder in cardholders)
                {
                  await  cardholder.SetCustomFieldAsync("Employee Number", "E00001");
                }

                var cards = engine.GetEntities(EntityType.Credential)
                     .OfType<Credential>()
                     .Where(c => c.Type == CredentialType.UndecodedWiegand)
                     .Select(c => c.Format)
                     .OfType<WiegandCredentialFormat>()
                     .Select(GetCardCode).ToList();

                RioCardDetected GetCardCode(WiegandCredentialFormat format)
                {
                    int charCount = (format.BitLength + 7) / 8 * 2;
                    int startIndex = format.RawData.Length - charCount;
                    var cardCode =  format.RawData.Substring(startIndex, charCount);
                    return new RioCardDetected("Address1", "R1", format.BitLength, cardCode);
                }

           
                RioClient client = new RioClient(Properties.Settings.Default.SoftwireServer, Properties.Settings.Default.SoftwireUsername, Properties.Settings.Default.SoftwirePassword);


                while (true)
                {
                    foreach (var item in cards)
                    {
                        client.CardSwipe("Channel1", item);
                    }

                    await Task.Delay(50);
                }

            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            async Task LoadEntities(params EntityType[] types)
            {
                Console.WriteLine("Loading entities...");

                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.AddRange(types);
                query.DownloadAllRelatedData = true;
                query.Page = 1;
                query.PageSize = 1000;

                QueryCompletedEventArgs args;

                do
                {
                    args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
                    query.Page++;

                } while (args.Error == ReportError.TooManyResults || args.Data.Rows.Count > query.PageSize);
            }
        }
    }
}
