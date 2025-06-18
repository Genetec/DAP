// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server;

using System;
using System.Collections.Generic;
using Genetec.Dap.CodeSamples.Properties;
using Genetec.Sdk.Plugin;

public class SamplePluginDescriptor : PluginDescriptor
{
    // TODO: Replace with your own unique plugin ID 
    public static Guid PluginId { get; } = new("4E8BB3F7-0D41-4F4C-A430-0B6EE7478CBE");

    public override string Description => Resources.PluginDescription;

    public override string Name => Resources.PluginName;

    public override Guid PluginGuid => PluginId;

    public override string SpecificDefaultConfig => null;

    public override bool IsSingleInstance => false;

    public override List<string> ApplicationId => new()
    {
        "KxsD11z743Hf5Gq9mv3+5ekxzemlCiUXkTFY5ba1NOGcLCmGstt2n0zYE9NsNimv" // Allow the plugin to run on a development system
        //TODO: Add your production SDK certificate application ID
    };
}
