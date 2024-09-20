// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples;

using Sdk;
using Sdk.Workspace.Modules;

public class SampleModule : Module
{
    public override void Load()
    {
        if (Workspace.ApplicationType is ApplicationType.SecurityDesk or ApplicationType.ConfigTool)
        {
            var task = new NotepadTask();
            task.Initialize(Workspace);
            Workspace.Tasks.Register(task);
        }
    }

    public override void Unload()
    {
    }
}