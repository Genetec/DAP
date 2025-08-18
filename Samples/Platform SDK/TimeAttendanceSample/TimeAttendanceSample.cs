// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

public class TimeAttendanceSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // TODO: Replace the following GUIDs with the actual GUIDs of the area and cardholder you want to query.
        Guid areaGuid = new Guid("ENTER_AREA_GUID_HERE");
        Guid[] cardholderGuids = { new("ENTER_CARDHOLDER_GUID_HERE") };
        TimeSpan startOfDay = TimeSpan.FromHours(9); // Start of day: 09:00 AM
        DateTime from = DateTime.Now.AddDays(-1); // Past day
        DateTime to = DateTime.Now;

        List<TimeAttendance> attendances = await GetTimeAndAttendance(engine, areaGuid, cardholderGuids, startOfDay, from, to);

        if (attendances.Any())
        {
            foreach (TimeAttendance attendance in attendances)
            {
                DisplayToConsole(engine, attendance);
            }
        }
        else
        {
            Console.WriteLine("No time and attendance records found for the specified criteria.");
        }
    }

    private async Task<List<TimeAttendance>> GetTimeAndAttendance(Engine engine, Guid area, IEnumerable<Guid> cardholders, TimeSpan startOfDay, DateTime from, DateTime to)
    {
        var query = (TimeAttendanceQuery)engine.ReportManager.CreateReportQuery(ReportType.TimeAttendanceActivity);

        query.Areas.Add(area);
        query.Cardholders.AddRange(cardholders);
        query.TimeRange.SetTimeRange(from, to);
        query.DayStartOffset = startOfDay;

        QueryCompletedEventArgs results = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

        return results.Data.AsEnumerable().Select(CreateTimeAttendance).ToList();

        TimeAttendance CreateTimeAttendance(DataRow row) => new()
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

    private void DisplayToConsole(Engine engine, TimeAttendance timeAttendance)
    {
        Console.WriteLine("Time and Attendance:");
        Console.WriteLine($"  Date: {timeAttendance.Date:yyyy-MM-dd}");
        Console.WriteLine($"  Cardholder: {GetEntityName(timeAttendance.CardholderGuid)}");
        Console.WriteLine($"  Area: {GetEntityName(timeAttendance.AreaGuid)}");
        Console.WriteLine($"  First Time In: {timeAttendance.FirstTimeIn:HH:mm:ss}");
        Console.WriteLine($"  Last Exit Time: {timeAttendance.LastExitTime?.ToString("HH:mm:ss") ?? "N/A"}");
        Console.WriteLine($"  Total Minutes: {timeAttendance.TotalMinutes}");
        Console.WriteLine($"  Total Minutes (Inclusive): {timeAttendance.TotalMinutesInclusive}");
        Console.WriteLine();

        string GetEntityName(Guid entityGuid) => engine.GetEntity(entityGuid)?.Name ?? "Unknown";
    }
}

class TimeAttendance
{
    public DateTime Date { get; set; }
    public Guid CardholderGuid { get; set; }
    public Guid AreaGuid { get; set; }
    public DateTime FirstTimeIn { get; set; }
    public DateTime? LastExitTime { get; set; }
    public int TotalMinutes { get; set; }
    public int TotalMinutesInclusive { get; set; }
}