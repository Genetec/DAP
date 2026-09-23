// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Client;

using Genetec.Sdk;
using Genetec.Sdk.Workspace.Modules;
using Genetec.Sdk.Workspace.Services;
using Genetec.Sdk.Workspace.Tasks;

/// <summary>
/// Registers the role configuration page in Config Tool and the custom-event report in Security Desk.
/// </summary>
public class SampleModule : Module
{
    public override void Load()
    {
        if (Workspace.ApplicationType == ApplicationType.SecurityDesk)
        {
            var task = new CreatePageTask<CustomEventsReportPage>();
            task.Initialize(Workspace);
            Workspace.Tasks.Register(task);
        }

        if (Workspace.ApplicationType == ApplicationType.ConfigTool)
        {
            // Initialize the custom configuration page and register it with the configuration service
            var page = new CustomConfigPage();
            page.Initialize(Workspace);
            Workspace.Services.Get<IConfigurationService>().Register(page);
        }
    }

    public override void Unload()
    {
    }
}
