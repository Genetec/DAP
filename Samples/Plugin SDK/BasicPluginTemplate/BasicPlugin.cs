// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using Sdk.EventsArgs;
    using Sdk.Plugin;

    [PluginProperty(typeof(BasicPluginDescriptor))]
    public class BasicPlugin : Plugin
    {
        protected override void OnPluginLoaded()
        {
        }

        protected override void OnPluginStart()
        {
            ModifyPluginState(new PluginStateEntry("PluginState", "Plugin started"));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        protected override void OnQueryReceived(ReportQueryReceivedEventArgs args)
        {
        }
    }
}