// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

// Run from the repository root:
// dotnet run --project Tests/MapObjectSample/MapObjectSample.Checks.csproj
using System;
using Genetec.Dap.CodeSamples;

var today = new DateTime(2026, 11, 4);
var cases = new (string Name, int? Inspection, int? Maintenance, int? Battery, int? Pads,
    bool InMaintenance, AedMaintenanceState Expected)[]
{
    ("All dates current", -10, 31, 31, 31, false, AedMaintenanceState.Current),
    ("Maintenance due today", -10, 0, 31, 31, false, AedMaintenanceState.Overdue),
    ("Battery due today", -10, 31, 0, 31, false, AedMaintenanceState.Overdue),
    ("Pads due today", -10, 31, 31, 0, false, AedMaintenanceState.Overdue),
    ("Past deadline", -10, 31, -1, 31, false, AedMaintenanceState.Overdue),
    ("Maintenance at warning boundary", -10, 30, 31, 31, false, AedMaintenanceState.DueSoon),
    ("Battery at warning boundary", -10, 31, 30, 31, false, AedMaintenanceState.DueSoon),
    ("Pads at warning boundary", -10, 31, 31, 30, false, AedMaintenanceState.DueSoon),
    ("Deadline tomorrow", -10, 1, 31, 31, false, AedMaintenanceState.DueSoon),
    ("No dates recorded", null, null, null, null, false, AedMaintenanceState.Unknown),
    ("Missing inspection", null, 31, 31, 31, false, AedMaintenanceState.Unknown),
    ("Missing date overrides due soon", -10, 30, null, 31, false, AedMaintenanceState.Unknown),
    ("Overdue overrides missing date", -10, null, 0, 31, false, AedMaintenanceState.Overdue),
    ("Future inspection", 1, 31, 31, 31, false, AedMaintenanceState.InvalidData),
    ("Maintenance equals inspection", -10, -10, 31, 31, false, AedMaintenanceState.InvalidData),
    ("Maintenance before inspection", -10, -11, 31, 31, false, AedMaintenanceState.InvalidData),
    ("Invalid overrides overdue and missing", 1, null, 0, 31, false, AedMaintenanceState.InvalidData),
    ("Maintenance mode overrides current", -10, 31, 31, 31, true, AedMaintenanceState.InMaintenance),
    ("Maintenance mode overrides all issues", 1, null, 0, 31, true, AedMaintenanceState.InMaintenance)
};

foreach (var test in cases)
{
    var information = new AedUnitInformation
    {
        LastInspectionDate = Date(test.Inspection),
        NextScheduledMaintenance = Date(test.Maintenance),
        BatteryExpirationDate = Date(test.Battery),
        PadExpirationDate = Date(test.Pads)
    };

    // Dates are calendar dates even when their stored time is later than the evaluation time.
    var actual = AedMaintenance.Evaluate(information, test.InMaintenance, today.AddHours(8));
    if (actual.State != test.Expected)
    {
        throw new Exception($"{test.Name}: expected {test.Expected}, got {actual.State}. {actual.Summary}");
    }
}

Console.WriteLine($"Passed {cases.Length} maintenance checks.");

DateTime Date(int? days) => days.HasValue ? today.AddDays(days.Value).AddHours(18) : DateTime.MinValue;
