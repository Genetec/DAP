// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Sdk.Workspace.Pages;

[Page(typeof(SamplePageDescriptor))]
public class SamplePage : Page
{
    public SamplePage() => View = new SamplePageView();
}