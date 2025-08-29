// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;

public record TimeAttendance
{
    public DateTime Date { get; init; }
    public Guid CardholderGuid { get; init; }
    public Guid AreaGuid { get; init; }
    public DateTime FirstTimeIn { get; init; }
    public DateTime? LastExitTime { get; init; }
    public int TotalMinutes { get; init; }
    public int TotalMinutesInclusive { get; init; }
}