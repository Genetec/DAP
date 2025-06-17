// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Client;

using Genetec.Sdk;
using Genetec.Sdk.Workspace.Modules;
using Genetec.Sdk.Workspace.Services;

public class SampleModule : Module
{
    public override void Load()
    {
        if (Workspace.ApplicationType == ApplicationType.ConfigTool) // Check if the application is the Config Tool
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
