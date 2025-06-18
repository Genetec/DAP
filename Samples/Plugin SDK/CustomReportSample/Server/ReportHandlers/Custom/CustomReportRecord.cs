// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers.Custom;

using System;

public class CustomReportRecord
{
    public Guid SourceId { get; set; }

    public int EventId { get; set; }

    public string Message { get; set; }

    public DateTime EventTimestamp { get; set; }

    public decimal Decimal { get; set; }

    public bool Boolean { get; set; }

    public byte[] Picture { get; set; }

    public string Hidden { get; set; }

    public int Numeric { get; set; }

    public TimeSpan Duration { get; set; }
}
