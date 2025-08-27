// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;

class CameraConfiguration
{
    public Guid Guid { get; set; }
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public string StreamUsage { get; set; }
    public string Schedule { get; set; }
    public string VideoFormat { get; set; }
    public string Resolution { get; set; }
    public string RecordingMode { get; set; }
    public string NetworkSetting { get; set; }
    public string MulticastAddress { get; set; }
    public string Port { get; set; }
    public string Trickling { get; set; }
    public int BitRate { get; set; }
    public int ImageQuality { get; set; }
    public int KeyFrameInterval { get; set; }
    public int KeyFrameIntervalUnits { get; set; }
    public int FrameRate { get; set; }
    public string UnitFirmwareVersion { get; set; }
    public string UnitIpAddress { get; set; }
    public int RetentionPeriod { get; set; }
}