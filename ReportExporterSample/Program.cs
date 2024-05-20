// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Genetec.Sdk.Queries;
    using Sdk;

    internal class Program
    {
        static Program()
        {
            SdkResolver.Initialize();
        }

        private static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var engine = new Engine();

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                var query = (CardholderActivityQuery)engine.ReportManager.CreateReportQuery(ReportType.CardholderActivity);
                query.MaximumResultCount = 10000;

                using var xmlWriter = new StreamWriter("CardholderActivity.xml", false, Encoding.UTF8);
                using var jsonWriter = new StreamWriter("CardholderActivity.json", false, Encoding.UTF8);
                using var csvWriter = new StreamWriter("CardholderActivity.csv", false, Encoding.UTF8);
                using var markDownWriter = new StreamWriter("CardholderActivity.md", false, Encoding.UTF8);
                using var htmlWriter = new StreamWriter("CardholderActivity.html", false, Encoding.UTF8);

                query.Exporter = new AggregateReportExporter(
                    new XmlReportExporter(xmlWriter), 
                    new JsonReportExporter(jsonWriter), 
                    new CsvReportExporter(csvWriter), 
                    new MarkdownReportExporter(markDownWriter),
                    new HtmlReportExporter(htmlWriter, "Cardholder Activity"));

               await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
