// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using Sdk.Plugin;

    public class BasicPluginDescriptor : PluginDescriptor
    {
        public override string Description => "Basic plugin's description";

        public override string Name => "Basic Plugin";

        // TODO: Replace with your own unique plugin GUID
        public override Guid PluginGuid => new Guid("84E87A98-8AD3-4853-85E3-05C86B9BF90C");

        public override string SpecificDefaultConfig => null;

        public override bool IsSingleInstance => true;

        public override List<string> ApplicationId => new List<string>
        {
            "KxsD11z743Hf5Gq9mv3+5ekxzemlCiUXkTFY5ba1NOGcLCmGstt2n0zYE9NsNimv", // Allow the plugin to run on a demo system
            //TODO: Add your SDK certificate application ID
        };
    }
}
