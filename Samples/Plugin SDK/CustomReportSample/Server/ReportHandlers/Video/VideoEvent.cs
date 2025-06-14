// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers.Video;

using System;
using Genetec.Sdk;

public class VideoEvent
{
    public Guid CameraGuid { get; set; }
    public Guid ArchiveSourceGuid { get; set; }
    public DateTime EventTime { get; set; }
    public EventType EventType { get; set; }
    public uint Value { get; set; }
    public string Notes { get; set; }
    public string XmlData { get; set; }
    public uint Capabilities { get; set; }
    public string TimeZone { get; set; }
    public byte[] Thumbnail { get; set; }
}