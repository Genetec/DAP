// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples.Client
{
    using System.Collections.Generic;
    using Genetec.Sdk;
    using Genetec.Sdk.Workspace.Pages;

    [Page(typeof(CustomPageDescriptor))]
    public class CustomReportPage : ReportPage
    {
        public override List<ReportField> Fields { get; } = new()
        {
            new ReportField { Type = ReportFieldType.Entity, Name = CustomReportColumnName.SourceId, DisplayName = "Source", IsSource = true },
            new ReportField { Type = ReportFieldType.DateTime, Name = CustomReportColumnName.EventTimestamp, DisplayName = "Timestamp", IsSource = true },
            new ReportField { Type = ReportFieldType.Event, Name = CustomReportColumnName.EventId, DisplayName = "Event" },
            new ReportField { Type = ReportFieldType.Text, Name = CustomReportColumnName.Message, DisplayName = "Message" },
            new ReportField { Type = ReportFieldType.Numeric, Name = CustomReportColumnName.Value, DisplayName = "Value" }
        };

        public override bool SupportsTiles => true;

        private readonly CustomReportFilter m_customReportFilter = new();

        protected override ReportFilter CustomFilter => m_customReportFilter;

        protected override bool DisplayEntityFilter => true;

        protected override bool DisplayTimeRangeFilter => true;

        protected override List<EntityType> EntityTypes { get; } = new() { EntityType.Camera, EntityType.VideoUnit };

        protected override void Deserialize(byte[] data)
        {
            // Optional: Deserialize your Custom report filter
        }

        protected override byte[] Serialize()
        {
            // Optional: Serialize your Custom report filter
            return null;
        }
    }
}