// Copyright (C) 2024 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using Sdk.Workspace.Pages;

    [Page(typeof(SamplePageDescriptor))]
    public class WebBrowserPage : Page
    {
        public WebBrowserPage()
        {
            View = new WebBrowserPageView();
        }
    }
}