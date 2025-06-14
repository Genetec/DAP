// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples;

using Genetec.Sdk.Workspace.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Workspace.Pages;

public class SampleModule : Sdk.Workspace.Modules.Module
{
    public override void Load()
    {
        switch (Workspace.ApplicationType)
        {
            case ApplicationType.SecurityDesk:
                RegisterCreatePageTask<SamplePage>();
                RegisterCreatePageTask<SampleTilePage>();
                break;
            case ApplicationType.ConfigTool:
                RegisterCreatePageTask<SamplePage>();
                break;
        }

        void RegisterCreatePageTask<T>() where T : Page
        {
            var task = new CreatePageTask<T>(); // Create a task that will open the page
            task.Initialize(Workspace);
            Workspace.Tasks.Register(task);
        }
    }

    public override void Unload()
    {
    }
}