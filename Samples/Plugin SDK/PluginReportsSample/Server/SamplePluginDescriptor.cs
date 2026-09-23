// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server;

using System;
using System.Collections.Generic;
using Genetec.Dap.CodeSamples.Properties;
using Genetec.Sdk.Plugin;

public class SamplePluginDescriptor : PluginDescriptor
{
    public override string Description => Resources.PluginDescription;

    public override string Name => Resources.PluginName;

    public override Guid PluginGuid => PluginTypes.SamplePlugin;

    public override string SpecificDefaultConfig => new RoleConfiguration().Serialize();

    // A single instance per system: two instances would compete for the same TCP port
    // when they run on the same server.
    public override bool IsSingleInstance => true;

    public override List<string> ApplicationId => new()
    {
        "KxsD11z743Hf5Gq9mv3+5ekxzemlCiUXkTFY5ba1NOGcLCmGstt2n0zYE9NsNimv" // Allow the plugin to run on a development system
        //TODO: Add your production SDK certificate application ID
    };
}
