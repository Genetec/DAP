// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk;
    using Sdk.Queries;

    internal class Program
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
                // Example parameters
                Guid areaGuid = new Guid("ENTER_AREA_GUID_HERE");
                List<Guid> cardholderGuids = new List<Guid> { new Guid("ENTER_CARDHOLDER_GUID_HERE") };
                TimeSpan? startOfDay = TimeSpan.FromHours(9); // Example start of day: 09:00 AM
                DateTime from = DateTime.UtcNow.AddDays(-1); // Example: past day
                DateTime to = DateTime.UtcNow;

                List<TimeAttendance> attendances = await GetTimeAndAttendance(engine, areaGuid, cardholderGuids, startOfDay, from, to);

                if (attendances.Any())
                {
                    foreach (var record in attendances)
                    {
                        Console.WriteLine($"Date: {record.Date}, Cardholder: {record.CardholderGuid}, Area: {record.AreaGuid}, First Time In: {record.FirstTimeIn}, Last Exit Time: {record.LastExitTime}, Total Minutes: {record.TotalMinutes}");
                    }
                }
                else
                {
                    Console.WriteLine("No time and attendance records found for the specified criteria.");
                }
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task<List<TimeAttendance>> GetTimeAndAttendance(Engine engine, Guid area, IEnumerable<Guid> cardholders, TimeSpan? startOfDay, DateTime from, DateTime to)
        {
            var query = (TimeAttendanceQuery)engine.ReportManager.CreateReportQuery(ReportType.TimeAttendanceActivity);

            query.Areas.Add(area);
            query.Cardholders.AddRange(cardholders);
            query.TimeRange.SetTimeRange(from, to);

            if (startOfDay.HasValue)
            {
                query.DayStartOffset = startOfDay.Value;
            }

            QueryCompletedEventArgs results = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            return results.Data.AsEnumerable().Select(CreateTimeAttendance).ToList();

            TimeAttendance CreateTimeAttendance(DataRow row) => new TimeAttendance
            {
                Date = DateTime.SpecifyKind(row.Field<DateTime>("Date"), DateTimeKind.Utc),
                CardholderGuid = row.Field<Guid>("CardholderGuid"),
                AreaGuid = row.Field<Guid>("AreaGuid"),
                FirstTimeIn = DateTime.SpecifyKind(row.Field<DateTime>("FirstTimeIn"), DateTimeKind.Utc),
                LastExitTime = row.IsNull("LastExitTime") ? (DateTime?)null : DateTime.SpecifyKind(row.Field<DateTime>("LastExitTime"), DateTimeKind.Utc),
                TotalMinutes = row.Field<int>("TotalMinutes"),
                TotalMinutesInclusive = row.Field<int>("TotalMinutesInclusive")
            };
        }
    }
}