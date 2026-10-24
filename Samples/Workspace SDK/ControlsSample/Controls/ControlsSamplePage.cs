// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk.Workspace.Pages;

[Page(typeof(ControlsSamplePageDescriptor))]
public class ControlsSamplePage : Page, IDisposable
{
    protected override void Initialize()
    {
        View = new ControlsSampleView(Workspace);
    }

    public void Dispose()
    {
        (View as IDisposable)?.Dispose();
    }
}
