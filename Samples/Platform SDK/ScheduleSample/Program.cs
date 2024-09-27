// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.Coverages;

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";

    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state == ConnectionStateCode.Success)
    {
        await engine.TransactionManager.ExecuteTransactionAsync(() =>
        {
            CreateEmployeeAccessSchedule(engine);
            CreateContractorAccessSchedule(engine);
            CreateSystemMaintenanceSchedule(engine);
            CreateAnnualEventSchedule(engine);
            CreateHolidaySchedule(engine);
        });
    }
    else
    {
        Console.WriteLine($"Logon failed: {state}");
    }

    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
}

void CreateEmployeeAccessSchedule(Engine engine)
{
    DailyCoverage coverage = (DailyCoverage)CoverageFactory.Instance.Create(CoverageType.Daily);
    coverage.Add(new DailyCoverageItem(new SdkTime(7, 0, 0), new SdkTime(19, 0, 0))); // 7 AM to 7 PM

    var schedule = (Schedule)engine.CreateEntity("Employee Access Hours", EntityType.Schedule);
    schedule.Coverage = coverage;
}

void CreateContractorAccessSchedule(Engine engine)
{
    WeeklyCoverage coverage = (WeeklyCoverage)CoverageFactory.Instance.Create(CoverageType.Weekly);

    foreach (var day in (DayOfWeek[])[DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday])
    {
        coverage.Add(new WeeklyCoverageItem(day, new SdkTime(8, 0, 0), new SdkTime(17, 0, 0))); // 8 AM to 5 PM
    }

    var schedule = (Schedule)engine.CreateEntity("Contractor Access", EntityType.Schedule);
    schedule.Coverage = coverage;
}

void CreateSystemMaintenanceSchedule(Engine engine)
{
    var coverage = (OrdinalCoverage)CoverageFactory.Instance.Create(CoverageType.Ordinal);

    var rangeCoverage = new RangeCoverage();
    rangeCoverage.Add(CoverageOffset.DayAfter, new DailyCoverageItem(new SdkTime(1, 0, 0), new SdkTime(3, 0, 0))); // 1 AM to 3 AM

    foreach (var month in (Months[])[Months.March, Months.June, Months.September])
    {
        coverage.Add(new OrdinalCoverageByDayOfMonthItem(1, month, rangeCoverage)); // 1st of each month
    }

    var schedule = (Schedule)engine.CreateEntity("System Maintenance Window", EntityType.Schedule);
    schedule.Coverage = coverage;
}

void CreateAnnualEventSchedule(Engine engine)
{
    var coverage = (OrdinalCoverage)CoverageFactory.Instance.Create(CoverageType.Ordinal);

    var rangeCoverage = new RangeCoverage();
    rangeCoverage.Add(CoverageOffset.DayAfter, new DailyCoverageItem(new SdkTime(6, 0, 0), new SdkTime(12, 0, 0))); // 6 AM to 12 PM

    coverage.Add(new OrdinalCoverageByDayOfWeekItem(DayOrdinal.First, DaySelection.Monday, Months.January, rangeCoverage)); // First Monday in January

    var schedule = (Schedule)engine.CreateEntity("Annual Event Access", EntityType.Schedule);
    schedule.Coverage = coverage;
}

void CreateHolidaySchedule(Engine engine)
{
    var coverage = (SpecificCoverage)CoverageFactory.Instance.Create(CoverageType.Specific);

    var rangeCoverage = new RangeCoverage();
    rangeCoverage.Add(CoverageOffset.CurrentDay, new DailyCoverageItem(new SdkTime(7, 0, 0), new SdkTime(15, 0, 0))); // 7 AM to 3 PM

    coverage.Add(new SpecificCoverageItemUnit(new SdkDate(2024, 12, 25), rangeCoverage)); // December 25, 2024, 7 AM to 3 PM

    var schedule = (Schedule)engine.CreateEntity("Christmas Day", EntityType.Schedule);
    schedule.Coverage = coverage;
}