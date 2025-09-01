// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Sdk;

public class SampleModule : Sdk.Workspace.Modules.Module
{
    public override void Load()
    {
        if (Workspace.ApplicationType == ApplicationType.SecurityDesk)
        {
            var builder = new SampleTileWidgetBuilder();
            builder.Initialize(Workspace);
            Workspace.Components.Register(builder);
        }
    }

    public override void Unload()
    {
    }
}