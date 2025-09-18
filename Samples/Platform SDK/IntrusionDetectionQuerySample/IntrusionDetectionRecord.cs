// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using Genetec.Sdk;

namespace Genetec.Dap.CodeSamples;

public record IntrusionDetectionRecord
{
    public DateTime Timestamp { get; init; }
    public EventType EventType { get; init; }
    public Guid IntrusionUnitId { get; init; }
    public Guid IntrusionAreaId { get; init; }
    public Guid DeviceId { get; init; }
    public Guid SourceGuid { get; init; }
    public int OccurrencePeriod { get; init; }
    public string TimeZoneId { get; init; }
    public Guid UserId { get; init; }
    public Guid CardholderId { get; init; }
    public Guid InitiatorId { get; init; }
}