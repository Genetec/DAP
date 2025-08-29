// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Sdk.Workspace.Pages;

[Page(typeof(ChartSamplePageDescriptor))]
public class ChartSamplePage : Page
{
    public ChartSamplePage()
    {
        View = new ChartSampleView();
    }
}