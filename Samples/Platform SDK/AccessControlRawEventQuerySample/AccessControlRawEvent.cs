// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using Genetec.Sdk;

namespace Genetec.Dap.CodeSamples;

public record AccessControlRawEvent
{
    public Guid? SourceGuid { get; init; }
    public Guid? AccessPointGuid { get; init; }
    public Guid? CredentialGuid { get; init; }
    public OfflinePeriodType OccurrencePeriod { get; init; }
    public Guid AccessManagerGuid { get; init; }
    public string TimeZone { get; init; }
    public long Position { get; init; }
    public DateTime InsertionTimestamp { get; init; }
    public DateTime Timestamp { get; init; }
    public EventType EventType { get; init; }
    public Guid? CardholderGuid { get; init; }
    public Guid? DeviceGuid { get; init; }
    public Guid? AccessPointGroupGuid { get; init; }
}