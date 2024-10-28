// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Genetec.Sdk.Queries;
    using Sdk;

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
