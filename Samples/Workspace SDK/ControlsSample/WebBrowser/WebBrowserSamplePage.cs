// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Sdk.Workspace.Pages;

[Page(typeof(WebBrowserSamplePageDescriptor))]
public class WebBrowserSamplePage : Page
{
    public WebBrowserSamplePage()
    {
        View = new WebBrowserSampleView();
    }
}