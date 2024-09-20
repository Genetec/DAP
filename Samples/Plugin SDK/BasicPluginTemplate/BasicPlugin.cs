// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples;

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