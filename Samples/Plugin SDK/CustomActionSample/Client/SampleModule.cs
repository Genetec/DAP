// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Client;

using Genetec.Sdk;
using Genetec.Sdk.Workspace.Components.CustomAction;
using Genetec.Sdk.Workspace.Modules;

public class SampleModule : Module
{
    static SampleModule() => AssemblyResolver.Initialize();

    public override void Load()
    {
        if (Workspace.ApplicationType is ApplicationType.ConfigTool or ApplicationType.SecurityDesk)
        {
            Register(new SampleCustomActionBuilder());
            Register(new HttpRequestActionBuilder());
        }

        void Register(CustomActionBuilder builder)
        {
            builder.Initialize(Workspace);
            Workspace.Components.Register(builder);
        }
    }

    public override void Unload()
    {
    }
}
