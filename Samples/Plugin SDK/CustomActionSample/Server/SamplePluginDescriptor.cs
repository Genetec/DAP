// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples.Server;

using System;
using System.Collections.Generic;
using Genetec.Sdk.Plugin;

public class SamplePluginDescriptor : PluginDescriptor
{
    public override string Description => "Custom action sample plugin";

    public override string Name => "Custom Action Sample";

    // TODO: Replace with your own unique plugin GUID
    public override Guid PluginGuid => PluginTypes.SamplePlugin;

    public override string SpecificDefaultConfig => null;

    public override bool IsSingleInstance => true;

    public override List<string> ApplicationId => new()
    {
        "KxsD11z743Hf5Gq9mv3+5ekxzemlCiUXkTFY5ba1NOGcLCmGstt2n0zYE9NsNimv", // Allow the plugin to run on a demo system
        //TODO: Add your SDK certificate application ID
    };
}