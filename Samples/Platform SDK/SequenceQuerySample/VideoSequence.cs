// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;

public record VideoSequence
{
    public Guid CameraGuid { get; init; }
    public Guid ArchiveSourceGuid { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public uint Capabilities { get; init; }
    public string TimeZone { get; init; }
    public Guid EncoderGuid { get; init; }
    public Guid UsageGuid { get; init; }
    public Guid OriginGuid { get; init; }
    public Guid MediaTypeGuid { get; init; }
    public int OriginType { get; init; }
}