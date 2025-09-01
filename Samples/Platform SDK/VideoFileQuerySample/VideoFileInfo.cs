// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk;

public record VideoFileInfo
{
    public Guid CameraGuid { get; init; }
    public Guid ArchiveSourceGuid { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string FilePath { get; init; }
    public decimal FileSize { get; init; }
    public string MetadataPath { get; init; }
    public VideoProtectionState ProtectionStatus { get; init; }
    public bool InfiniteProtection { get; init; }
    public string Drive { get; init; }
    public uint Error { get; init; }
    public DateTime ProtectionEndDateTime { get; init; }
}