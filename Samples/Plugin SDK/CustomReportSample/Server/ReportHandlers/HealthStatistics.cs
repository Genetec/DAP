// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using System;

public class HealthStatistics
{
    public int FailureCount { get; set; }
    public int RtpPacketLoss { get; set; }
    public int CalculationStatus { get; set; }
    public Guid SourceEntityGuid { get; set; }
    public int EventSourceType { get; set; }
    public TimeSpan UnexpectedDowntime { get; set; }
    public TimeSpan ExpectedDowntime { get; set; }
    public TimeSpan Uptime { get; set; }
    public float Mttr { get; set; }
    public float Mtbf { get; set; }
    public float Availability { get; set; }
    public DateTime LastErrorTimestamp { get; set; }
    public Guid ObserverEntity { get; set; }
}