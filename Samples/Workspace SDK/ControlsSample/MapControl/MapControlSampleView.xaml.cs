// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Genetec.Sdk.Workspace;

public partial class MapControlSampleView
{
    public MapControlSampleView(Workspace workspace)
    {
        InitializeComponent();
        DataContext = new MapControlSampleViewModel(workspace);
    }
}