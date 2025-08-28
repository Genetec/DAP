// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Sdk.Workspace.Pages;

[Page(typeof(MapControlSamplePageDescriptor))]
public class MapControlSamplePage : Page
{
    protected override void Initialize()
    {
        View = new MapControlSampleView(Workspace);
    }
}