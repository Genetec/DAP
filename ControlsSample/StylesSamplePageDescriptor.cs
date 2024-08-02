// Copyright (C) 2024 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Sdk.Workspace.Pages;

    public class StylesSamplePageDescriptor : PageDescriptor
    {
        public override string Name => "Controls Style Sample";

        public override Guid Type { get; } = new("99470468-048A-4511-878B-CDDDE2B18AA7");

        public override string Description => "This page provides samples of the available control styles.";

        public override Guid CategoryId => CustomTaskCategories.SdkControls;

        public override TaskIconColor IconColor => TaskIconColor.DefaultIconColor;

        public override bool HasPrivilege() => true;
    }
}