// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;

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