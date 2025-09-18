// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using Genetec.Sdk;

namespace Genetec.Dap.CodeSamples;

public record VideoEventRecord
{
    public Guid CameraGuid { get; init; }
    public Guid ArchiveSourceGuid { get; init; }
    public DateTime EventTime { get; init; }
    public EventType EventType { get; init; }
    public uint Value { get; init; }
    public string Notes { get; init; }
    public string XMLData { get; init; }
    public uint Capabilities { get; init; }
    public string TimeZone { get; init; }
}