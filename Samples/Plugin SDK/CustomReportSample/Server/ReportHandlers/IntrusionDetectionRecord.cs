// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using System;

public class IntrusionDetectionRecord
{
    public DateTime Timestamp { get; set; }
    public int EventType { get; set; }
    public Guid IntrusionUnitId { get; set; }
    public Guid IntrusionAreaId { get; set; }
    public Guid DeviceId { get; set; }
    public Guid SourceGuid { get; set; }
    public int OccurrencePeriod { get; set; }
    public string TimeZoneId { get; set; }
    public Guid InitiatorId { get; set; }
}