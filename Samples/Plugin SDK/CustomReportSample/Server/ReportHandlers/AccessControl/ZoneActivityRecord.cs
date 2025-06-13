// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers.AccessControl;

using System;

public class ZoneActivityRecord
{
    public DateTime Timestamp { get; set; }
    public int EventType { get; set; }
    public int EventId { get; set; }
    public DateTime TimestampLocal { get; set; }
    public string TimeZoneId { get; set; }
    public Guid ZoneId { get; set; }
    public int OfflinePeriod { get; set; }
}