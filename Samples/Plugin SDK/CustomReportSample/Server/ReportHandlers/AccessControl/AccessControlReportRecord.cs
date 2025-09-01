// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers.AccessControl;

using System;
using Genetec.Sdk;

public class AccessControlReportRecord
{
    public DateTime Timestamp { get; set; }
    public EventType EventType { get; set; }
    public Guid UnitGuid { get; set; }
    public Guid DeviceGuid { get; set; }
    public Guid APGuid { get; set; }
    public Guid SourceGuid { get; set; }
    public Guid CredentialGuid { get; set; }
    public Guid CardholderGuid { get; set; }
    public Guid Credential2Guid { get; set; }
    public string TimeZone { get; set; }
    public int OccurrencePeriod { get; set; }
    public Guid AccessPointGroupGuid { get; set; }
    public string CustomEventMessage { get; set; }
}