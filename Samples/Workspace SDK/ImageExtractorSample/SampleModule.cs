// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Sdk.Workspace.Modules;

public class SampleModule : Module
{
    static SampleModule() => AssemblyResolver.Initialize();

    public override void Load()
    {
        var component = new SampleImageExtractor();
        component.Initialize(Workspace);
        Workspace.Components.Register(component);
    }

    public override void Unload()
    {
    }
}