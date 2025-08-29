// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Client;

using Genetec.Sdk;
using Genetec.Sdk.Workspace.Modules;
using Genetec.Sdk.Workspace.Tasks;

/// <summary>
/// Client module for the custom report sample.
/// </summary>
public class SampleModule : Module
{
    public override void Load()
    {
        // Register the custom report page task for Security Desk only
        if (Workspace.ApplicationType == ApplicationType.SecurityDesk)
        {
            var task = new CreatePageTask<CustomReportPage>();
            task.Initialize(Workspace);
            Workspace.Tasks.Register(task);
        }
    }

    public override void Unload()
    {
    }
}