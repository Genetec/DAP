// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using Sdk.Workspace.Pages;

    [Page(typeof(StylesSamplePageDescriptor))]
    public class StylesSamplePage : Page
    {
        public StylesSamplePage()
        {
            View = new StylesSamplePageView();
        }
    }
}
