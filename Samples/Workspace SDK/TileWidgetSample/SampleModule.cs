// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

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