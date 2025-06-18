// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using Sdk.Workspace.Pages;

    [Page(typeof(ControlsSamplePageDescriptor))]
    public class ControlsSamplePage : Page
    {
        public ControlsSamplePage()
        {
            View = new ControlsSampleView();
        }
    }
}
