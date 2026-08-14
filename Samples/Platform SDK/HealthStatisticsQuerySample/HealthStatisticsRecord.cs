// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using Genetec.Sdk;

namespace Genetec.Dap.CodeSamples;

public record HealthStatisticsRecord
{
    public Guid SourceEntityGuid { get; init; }
    public EventSourceType EventSourceType { get; init; }
    public Guid ObserverEntity { get; init; }
    public float Availability { get; init; }
    public TimeSpan Uptime { get; init; }
    public TimeSpan ExpectedDowntime { get; init; }
    public TimeSpan UnexpectedDowntime { get; init; }
    public float Mtbf { get; init; }
    public float Mttr { get; init; }
    public int FailureCount { get; init; }
    public int RtpPacketLoss { get; init; }
    public AvailabilityCalculationStatus CalculationStatus { get; init; }
    public DateTime LastErrorTimestamp { get; init; }
    public DateTime LastSeenOnline { get; init; }
}
