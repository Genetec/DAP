// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using Genetec.Sdk;

namespace Genetec.Dap.CodeSamples;

public record AccessControlEvent
{
    public DateTime Timestamp { get; init; }
    public Guid? Unit { get; init; }
    public Guid? AccessPoint { get; init; }
    public Guid? AccessPointGroup { get; init; }
    public Guid? Credential { get; init; }
    public Guid? Credential2 { get; init; }
    public Guid? Device { get; init; }
    public string CustomEventMessage { get; init; }
    public EventType EventType { get; init; }
    public Guid? Source { get; init; }
    public TimeZoneInfo TimeZone { get; init; }
    public OfflinePeriodType OccurrencePeriod { get; init; }
    public Guid? Cardholder { get; init; }
}