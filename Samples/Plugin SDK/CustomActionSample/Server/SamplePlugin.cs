// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples.Server;

using Genetec.Sdk;
using Genetec.Sdk.Actions.CustomAction;
using Genetec.Sdk.Entities;
using Genetec.Sdk.EventsArgs;
using Genetec.Sdk.Plugin;

[PluginProperty(typeof(SamplePluginDescriptor))]
public class SamplePlugin : Plugin
{
    protected override void OnPluginLoaded()
    {
        // Add a handler for the action received event.
        Engine.ActionReceived += OnActionReceived;
    }
        
    protected override void OnPluginStart()
    {
        CustomActionTypeDescriptor descriptor = new(CustomActionTypes.LaunchEncoderCommand, "Launch encoder command");
        descriptor.Description = "Launches an encoder command on a camera";
        descriptor.SupportedActionUsage = ActionUsage.All;
        descriptor.HandleByServer = true;
        descriptor.SetIcon(Properties.Resources.SmallLogo);

        var config = (SystemConfiguration)Engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);
        config.AddOrUpdateCustomActionType(descriptor);

        ModifyPluginState(new PluginStateEntry("PluginState", "Plugin started"));
    }

    private void OnActionReceived(object sender, ActionReceivedEventArgs e)
    {
        // Check if the action is a custom action of type LaunchEncoderCommand.
        if (e.ActionType == ActionType.CustomAction && e.Action is CustomAction customAction
                                                    && customAction.CustomActionType == CustomActionTypes.LaunchEncoderCommand)
        {
            Logger.TraceDebug("Received a LaunchEncoderCommand action");

            // Deserialize the payload to get the camera and the encoder command.
            LaunchEncoderCommandAction payload = LaunchEncoderCommandAction.Deserialize(customAction.Payload);

            // Get the camera entity and launch the encoder command.
            if (payload != null && Engine.GetEntity(payload.Camera) is Camera camera)
            {
                Logger.TraceDebug($"Launching encoder command {payload.EncoderCommand} on camera {camera.Name}");
                camera.LaunchEncoderCommand(payload.EncoderCommand);
            }
        }
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