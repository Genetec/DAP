// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Sdk.Workspace.Pages;

    public class WebBrowserSamplePageDescriptor : PageDescriptor
    {
        public override string Name => "Web Browser Sample";

        public override Guid Type { get; } = new("F5866920-2AF8-423B-BBE3-5F9A07C28CE9");

        public override Guid CategoryId => CustomTaskCategories.SdkSamples;

        public override string Description => "This page provides a sample of the WebBrowser control.";

        public override TaskIconColor IconColor => TaskIconColor.DefaultIconColor;

        public override bool HasPrivilege() => true;
    }
}
