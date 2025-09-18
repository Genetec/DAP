// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using Genetec.Sdk;

namespace Genetec.Dap.CodeSamples;

public record HitRecord
{
    public Guid HitId { get; init; }
    public Guid LprRuleId { get; init; }
    public DateTime Timestamp { get; init; }
    public DateTime TimestampUtc { get; init; }
    public string TimeZoneId { get; init; }
    public int UserActionType { get; init; }
    public HitType HitType { get; init; }
    public HitReason ReasonType { get; init; }
    public string ExtraInfo { get; init; }
    public Guid Read1Id { get; init; }
    public string Read1Plate { get; init; }
    public string Read1PlateState { get; init; }
    public DateTime Read1Timestamp { get; init; }
    public DateTime Read1TimestampUtc { get; init; }
    public Guid Read1UnitId { get; init; }
    public Guid Read1PatrollerId { get; init; }
}