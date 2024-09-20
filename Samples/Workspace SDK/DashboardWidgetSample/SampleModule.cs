// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples;

using Sdk;
using Sdk.Workspace.Modules;

public class SampleModule : Module
{
    public override void Load()
    {
        if (Workspace.ApplicationType == ApplicationType.SecurityDesk)
        {
            var builder = new ClockWidgetBuilder();
            builder.Initialize(Workspace);
            Workspace.Components.Register(builder);
        }
    }

    public override void Unload()
    {
    }
}