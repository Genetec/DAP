// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.AccessControl
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk;
    using Properties;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            var engine = new Engine();
            engine.LoginManager.ConnectionRetry = -1;
            engine.LoginManager.LogonStatusChanged += (sender, args) => Console.WriteLine($"Logon status: {args.Status}");
            engine.LoginManager.LogonFailed += (sender, args) => Console.WriteLine($"Logon failed: {args.FailureCode}");

            ConnectionStateCode state = await engine.LogOnAsync(Settings.Default.Server, Settings.Default.Username, Settings.Default.Password);
            if (state == ConnectionStateCode.Success)
            {
                try
                {
                    var numberReader = new EmployeeNumberReader(Settings.Default.ConnectionString);
                    var repository = new EventRecordRepository(Settings.Default.ConnectionString);
                    var extractor = new EventExtractor(engine, repository, numberReader);

                    const string format = "yyyy-MM-ddTHH:mm:ss";

                    if (!DateTime.TryParseExact(Settings.Default.StartDate, format, null, System.Globalization.DateTimeStyles.AssumeLocal, out DateTime startDate))
                    {
                        startDate = DateTime.MinValue;
                    }

                    if (!DateTime.TryParseExact(Settings.Default.EndDate, format, null, System.Globalization.DateTimeStyles.AssumeLocal, out DateTime endDate))
                    {
                        endDate = DateTime.MinValue;
                    }

                    var eventTypes = Settings.Default.EventTypes.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(value => (EventType)Enum.Parse(typeof(EventType), value.Trim(), true));
                    
                    await extractor.ExtractData(startDate, endDate, eventTypes);

                    Console.WriteLine("Extraction completed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Extraction failed: {ex.Message}");
                }
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
