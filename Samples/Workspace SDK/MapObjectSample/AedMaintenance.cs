// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Linq;

public enum AedMaintenanceState
{
    Current,
    DueSoon,
    Unknown,
    Overdue,
    InvalidData,
    InMaintenance
}

public sealed class AedMaintenance
{
    public const int WarningDays = 30;

    public AedMaintenanceState State { get; private set; }
    public string Summary { get; private set; }

    private static bool IsKnown(DateTime date)
    {
        return date != DateTime.MinValue;
    }

    // These dates describe recorded maintenance, not the AED's physical readiness.
    public static AedMaintenance Evaluate(AedUnitInformation information, bool inMaintenance, DateTime today)
    {
        if (information == null)
        {
            throw new ArgumentNullException(nameof(information));
        }

        today = today.Date;
        var issues = new List<string>();
        var missing = new List<string>();
        var deadlines = new[]
        {
            (Name: "Maintenance", Date: information.NextScheduledMaintenance),
            (Name: "Battery", Date: information.BatteryExpirationDate),
            (Name: "Pads", Date: information.PadExpirationDate)
        };

        foreach (var (Name, Date) in deadlines)
        {
            if (!IsKnown(Date))
            {
                missing.Add(Name);
            }
            else if (Date.Date <= today)
            {
                issues.Add(Name + " due " + Date.ToString("yyyy-MM-dd"));
            }
            else if (Date.Date <= today.AddDays(WarningDays))
            {
                issues.Add(Name + " due within " + WarningDays + " days (" + Date.ToString("yyyy-MM-dd") + ")");
            }
        }
        if (!IsKnown(information.LastInspectionDate))
        {
            missing.Add("Last inspection");
        }

        var invalid = Validate(information, today);
        // Apply more serious states last so they take precedence.
        var state = AedMaintenanceState.Current;

        if (issues.Count != 0)
        {
            state = AedMaintenanceState.DueSoon;
        }

        if (missing.Count != 0)
        {
            state = AedMaintenanceState.Unknown;
        }

        if (deadlines.Any(item => IsKnown(item.Date) && item.Date.Date <= today))
        {
            state = AedMaintenanceState.Overdue;
        }

        if (invalid != null)
        {
            state = AedMaintenanceState.InvalidData;
        }

        if (inMaintenance)
        {
            state = AedMaintenanceState.InMaintenance;
        }

        if (inMaintenance)
        {
            issues.Insert(0, "Entity is in Security Center maintenance mode");
        }

        if (invalid != null)
        {
            issues.Add(invalid);
        }

        if (missing.Count != 0)
        {
            issues.Add("Dates not recorded: " + string.Join(", ", missing));
        }

        return new AedMaintenance
        {
            State = state,
            Summary = issues.Count == 0 ? "Recorded dates are current. Physical readiness is not verified." : string.Join("; ", issues)
        };
    }

    private static string Validate(AedUnitInformation information, DateTime today)
    {
        return IsKnown(information.LastInspectionDate) && information.LastInspectionDate.Date > today.Date
            ? "The last inspection cannot be in the future."
            : IsKnown(information.LastInspectionDate) && IsKnown(information.NextScheduledMaintenance) &&
            information.NextScheduledMaintenance.Date <= information.LastInspectionDate.Date
            ? "The next maintenance date must be after the last inspection."
            : null;
    }
}
