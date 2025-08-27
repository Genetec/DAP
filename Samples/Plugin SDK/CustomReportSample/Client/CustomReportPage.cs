// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Client;

using System.Collections.Generic;
using Genetec.Sdk;
using Genetec.Sdk.Workspace.Pages;

[Page(typeof(CustomReportPageDescriptor))]
public class CustomReportPage : ReportPage
{
    private readonly CustomReportFilter m_customReportFilter = new();

    public override List<ReportField> Fields { get; } = new()
    {
        new ReportField { Type = ReportFieldType.Entity, Name = CustomReportColumnName.SourceId, DisplayName = "Source", IsSource = true },
        new ReportField { Type = ReportFieldType.DateTime, Name = CustomReportColumnName.EventTimestamp, DisplayName = "Timestamp", IsSource = true },
        new ReportField { Type = ReportFieldType.Event, Name = CustomReportColumnName.EventId, DisplayName = "Event" },
        new ReportField { Type = ReportFieldType.Text, Name = CustomReportColumnName.Message, DisplayName = "Message" },
        new ReportField { Type = ReportFieldType.Numeric, Name = CustomReportColumnName.Numeric, DisplayName = "Numeric" },
        new ReportField { Type = ReportFieldType.Boolean, Name = CustomReportColumnName.Boolean, DisplayName = "Boolean" },
        new ReportField { Type = ReportFieldType.Decimal, Name = CustomReportColumnName.Decimal, DisplayName = "Decimal" },
        new ReportField { Type = ReportFieldType.TimeSpan, Name = CustomReportColumnName.Duration, DisplayName = "Duration" },
        new ReportField { Type = ReportFieldType.Image, Name = CustomReportColumnName.Picture, DisplayName = "Picture", ImageMaxHeight=256, ImageMaxWidth=256, InitialWidth=128 },
        new ReportField { Type = ReportFieldType.Text, Name = CustomReportColumnName.Hidden, IsVisible = false } // Hidden field
    };

    // Show the tiles
    public override bool SupportsTiles => true;

    protected override ReportFilter CustomFilter => m_customReportFilter;

    // Display the time range filter
    protected override bool DisplayTimeRangeFilter => true;

    // Display the entity filter
    protected override bool DisplayEntityFilter => true;

    // Entity types that can be selected in the entity filter
    protected override List<EntityType> EntityTypes { get; } = new() { EntityType.Cardholder };
}