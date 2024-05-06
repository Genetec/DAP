// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System.Collections.Generic;
    using Genetec.Sdk;
    using Genetec.Sdk.Workspace.Pages;

    [Page(typeof(CustomPageDescriptor))]
    public class CustomReportPage : ReportPage
    {
        public override List<ReportField> Fields { get; } = new List<ReportField>
        {
            new ReportField { Type = ReportFieldType.Entity, Name = "SourceId", DisplayName = "Source", IsSource = true },
            new ReportField { Type = ReportFieldType.DateTime, Name = "EventTimestamp", DisplayName = "Timestamp", IsSource = true },
            new ReportField { Type = ReportFieldType.Event, Name = "EventId", DisplayName = "Event" },
            new ReportField { Type = ReportFieldType.Text, Name = "Message", DisplayName = "Message" },
            new ReportField { Type = ReportFieldType.Numeric, Name = "Value", DisplayName = "Value" }
        };

        public override bool SupportsTiles => true;

        private readonly CustomReportFilter m_customReportFilter = new CustomReportFilter();

        protected override ReportFilter CustomFilter => m_customReportFilter;

        protected override bool DisplayEntityFilter => true;

        protected override bool DisplayTimeRangeFilter => true;

        protected override List<EntityType> EntityTypes { get; } = new List<EntityType> { EntityType.Camera, EntityType.VideoUnit };

        protected override void Deserialize(byte[] data)
        {
        }

        protected override byte[] Serialize()
        {
            return null;
        }
    }
}