// Copyright (C) 2024 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using Sdk.Workspace.Pages;

    [Page(typeof(StylesSamplePageDescriptor))]
    public class StylesSamplePage : Page
    {
        public StylesSamplePage() => View = new ControlsSamplePageView();
    }
}