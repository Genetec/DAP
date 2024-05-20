// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using Sdk.Plugin;

    public class SamplePluginDescriptor : PluginDescriptor
    {
        public override string Description => "A plugin with database support";

        public override string Name => "Sample Plugin Database";

        // TODO: Replace with your own unique plugin GUID
        public override Guid PluginGuid => new Guid("266B2366-26AA-4538-9953-FC435D3063EE");

        public override string SpecificDefaultConfig => null;

        public override List<string> ApplicationId => new List<string>
        {
            "KxsD11z743Hf5Gq9mv3+5ekxzemlCiUXkTFY5ba1NOGcLCmGstt2n0zYE9NsNimv", // Allow the plugin to run on a demo system
            //TODO: Add your SDK certificate application ID
        };
    }
}