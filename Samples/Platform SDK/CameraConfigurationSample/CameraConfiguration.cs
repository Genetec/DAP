// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;

public record CameraConfiguration
{
    public Guid Guid { get; init; }
    public string Manufacturer { get; init; }
    public string Model { get; init; }
    public string StreamUsage { get; init; }
    public string Schedule { get; init; }
    public string VideoFormat { get; init; }
    public string Resolution { get; init; }
    public string RecordingMode { get; init; }
    public string NetworkSetting { get; init; }
    public string MulticastAddress { get; init; }
    public string Port { get; init; }
    public string Trickling { get; init; }
    public int BitRate { get; init; }
    public int ImageQuality { get; init; }
    public int KeyFrameInterval { get; init; }
    public int KeyFrameIntervalUnits { get; init; }
    public int FrameRate { get; init; }
    public string UnitFirmwareVersion { get; init; }
    public string UnitIpAddress { get; init; }
    public int RetentionPeriod { get; init; }
}