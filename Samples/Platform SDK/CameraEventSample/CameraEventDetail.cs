// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Genetec.Sdk;

public record CameraEventDetail
{
    public Guid CameraGuid { get; init; }
    public Guid ArchiveSourceGuid { get; init; }
    public DateTime EventTime { get; init; }
    public EventType EventType { get; init; }
    public uint Value { get; init; }
    public string Notes { get; init; }
    public string XmlData { get; init; }
    public uint Capabilities { get; init; }
    public string TimeZone { get; init; }
    public byte[] Thumbnail { get; init; }
}