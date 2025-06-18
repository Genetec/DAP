// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Sdk.Workspace.Pages;

    public class ChartSamplePageDescriptor : PageDescriptor
    {
        public override string Name => "Chart Sample";

        public override Guid Type { get; } = new("58FE9C39-5054-458D-9B22-2FD42FE3C224");

        public override string Description => "This page provides a sample of the Chart control.";

        public override Guid CategoryId => CustomTaskCategories.SdkSamples;

        public override TaskIconColor IconColor => TaskIconColor.DefaultIconColor;

        public override bool HasPrivilege() => true;
    }
}
