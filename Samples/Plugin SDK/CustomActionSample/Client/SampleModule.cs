// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples.Client;

using Genetec.Sdk;
using Genetec.Sdk.Workspace.Modules;

public class SampleModule : Module
{
    static SampleModule() => AssemblyResolver.Initialize();

    public override void Load()
    {
        if (Workspace.ApplicationType is ApplicationType.ConfigTool or ApplicationType.SecurityDesk)
        {
            SampleCustomActionBuilder builder = new();
            builder.Initialize(Workspace);
            Workspace.Components.Register(builder);
        }
    }

    public override void Unload()
    {
    }
}