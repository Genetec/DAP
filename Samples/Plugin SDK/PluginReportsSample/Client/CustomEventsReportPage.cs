// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Genetec.Sdk;
using Genetec.Sdk.Workspace.Pages;
using Genetec.Sdk.Workspace.Tasks;

[Page(typeof(CustomEventsReportDescriptor))]
public sealed class CustomEventsReportPage : ReportPage
{
    private readonly CustomEventsReportFilter m_filter = new();

    public override List<ReportField> Fields { get; } = new()
    {
        new ReportField { Type = ReportFieldType.Entity, Name = CustomEventReport.Source, DisplayName = "Source", IsSource = true },
        new ReportField { Type = ReportFieldType.DateTime, Name = CustomEventReport.Timestamp, DisplayName = "Timestamp" },
        new ReportField { Type = ReportFieldType.Event, Name = CustomEventReport.Event, DisplayName = "Custom event" },
        new ReportField { Type = ReportFieldType.Text, Name = CustomEventReport.Message, DisplayName = "Message" },
        new ReportField { Type = ReportFieldType.Text, Name = CustomEventReport.ExtraHiddenPayload, DisplayName = "Extra hidden payload", IsVisible = false }
    };

    protected override ReportFilter CustomFilter => m_filter;
    protected override bool DisplayTimeRangeFilter => true;
    protected override bool DisplayEntityFilter => true;
    protected override bool DisplayEventFilter => false;
    protected override List<EntityType> EntityTypes => CustomEventReport.GetDefinitions(Workspace.Sdk)
        .Select(item => item.SourceEntityType).Distinct().DefaultIfEmpty(EntityType.None).ToList();
}

public sealed class CustomEventsReportDescriptor : PageDescriptor
{
    public override Guid Type => CustomEventReport.Id;
    public override string Name => "Custom event activities";
    public override string Description => "Find custom event activities by event, source, time range, and message.";
    public override Guid CategoryId => new(TaskCategories.Investigation);
    public override bool AllowOfflineExecution => false;
    public override ImageSource Icon { get; } = new BitmapImage(new Uri("pack://application:,,,/PluginReportsSample;component/Resources/Images/SmallLogo.png"));
    public override ImageSource Thumbnail { get; } = new BitmapImage(new Uri("pack://application:,,,/PluginReportsSample;component/Resources/Images/LargeLogo.png"));
    public override bool HasPrivilege() => true;
}
