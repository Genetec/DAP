// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;

namespace Genetec.Dap.CodeSamples;

public record HealthEventRecord
{
    public int HealthEventId { get; init; }
    public int EventSourceTypeId { get; init; }
    public Guid? SourceEntityGuid { get; init; }
    public string EventDescription { get; init; }
    public string MachineName { get; init; }
    public DateTime Timestamp { get; init; }
    public int SeverityId { get; init; }
}