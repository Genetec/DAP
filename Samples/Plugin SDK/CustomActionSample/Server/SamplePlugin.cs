// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Actions.CustomAction;
using Genetec.Sdk.Entities;
using Genetec.Sdk.EventsArgs;
using Genetec.Sdk.Plugin;

[PluginProperty(typeof(SamplePluginDescriptor))]
public class SamplePlugin : Plugin
{
    private static readonly HttpClient s_httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

    // The static constructor initializes the AssemblyResolver to ensure that
    // the plugin can dynamically resolve and load required assemblies at runtime.
    static SamplePlugin() => AssemblyResolver.Initialize();

    protected override void OnPluginLoaded()
    {
        // Add a handler for the action received event.
        Engine.ActionReceived += OnActionReceived;
    }

    protected override void OnPluginStart()
    {
        var config = (SystemConfiguration)Engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);
        RegisterAction(CustomActionTypes.LaunchEncoderCommand, "Launch encoder command", "Launches an encoder command on a camera");
        RegisterAction(CustomActionTypes.SendHttpRequest, "Send HTTP request", "Sends an HTTP request to a configured endpoint");

        ModifyPluginState(new PluginStateEntry("PluginState", "Plugin started"));

        void RegisterAction(Guid id, string name, string description)
        {
            var descriptor = new CustomActionTypeDescriptor(id, name)
            {
                Description = description,
                SupportedActionUsage = ActionUsage.All,
                HandleByServer = true
            };
            descriptor.SetIcon(Properties.Resources.SmallLogo);
            config.AddOrUpdateCustomActionType(descriptor);
        }
    }

    private void OnActionReceived(object sender, ActionReceivedEventArgs e)
    {
        if (e.ActionType != ActionType.CustomAction || e.Action is not CustomAction customAction)
        {
            return;
        }

        if (customAction.CustomActionType == CustomActionTypes.LaunchEncoderCommand)
        {
            HandleLaunchEncoderCommand(customAction.Payload);
        }
        else if (customAction.CustomActionType == CustomActionTypes.SendHttpRequest)
        {
            HandleSendHttpRequest(customAction.Payload);
        }
    }

    private void HandleLaunchEncoderCommand(string payload)
    {
        Logger.TraceDebug("Received a LaunchEncoderCommand action");

        LaunchEncoderCommandAction action = LaunchEncoderCommandAction.Deserialize(payload);
        if (action != null && Engine.GetEntity(action.Camera) is Camera camera)
        {
            Logger.TraceDebug($"Launching encoder command {action.EncoderCommand} on camera {camera.Name}");
            camera.LaunchEncoderCommand(action.EncoderCommand);
        }
    }

    private void HandleSendHttpRequest(string payload)
    {
        Logger.TraceDebug("Received a SendHttpRequest action");

        SendHttpRequestAction action = SendHttpRequestAction.Deserialize(payload);
        if (action != null)
        {
            _ = SendRequestAsync(action);
        }
    }

    private async Task SendRequestAsync(SendHttpRequestAction action)
    {
        try
        {
            using HttpRequestMessage request = BuildRequest(action);
            using HttpResponseMessage response = await s_httpClient.SendAsync(request).ConfigureAwait(false);
            Logger.TraceInformation($"HTTP {action.Method} returned {(int)response.StatusCode} {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            Logger.TraceError(ex, $"HTTP {action.Method} request failed");
        }
    }

    private static HttpRequestMessage BuildRequest(SendHttpRequestAction action)
    {
        if (!Uri.TryCreate(action.Url, UriKind.Absolute, out Uri endpoint)
            || (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("The URL must be an absolute HTTP or HTTPS URL.");
        }

        var builder = new UriBuilder(endpoint);
        string query = BuildQueryString(action.QueryParameters ?? Enumerable.Empty<NameValuePair>());
        if (!string.IsNullOrEmpty(query))
        {
            string existingQuery = builder.Query.TrimStart('?');
            builder.Query = string.IsNullOrEmpty(existingQuery) ? query : $"{existingQuery}&{query}";
        }

        var request = new HttpRequestMessage(new HttpMethod(action.Method), builder.Uri);
        if (!string.IsNullOrEmpty(action.Body) && action.Method is "POST" or "PUT" or "PATCH")
        {
            request.Content = new StringContent(action.Body, Encoding.UTF8,
                string.IsNullOrEmpty(action.ContentType) ? "application/json" : action.ContentType);
        }

        foreach (NameValuePair header in action.Headers ?? Enumerable.Empty<NameValuePair>())
        {
            if (!string.IsNullOrEmpty(header.Name))
            {
                request.Headers.TryAddWithoutValidation(header.Name, header.Value);
            }
        }

        return request;
    }

    private static string BuildQueryString(IEnumerable<NameValuePair> parameters)
    {
        return string.Join("&", parameters
            .Where(parameter => !string.IsNullOrEmpty(parameter.Name))
            .Select(parameter => $"{Uri.EscapeDataString(parameter.Name)}={Uri.EscapeDataString(parameter.Value ?? string.Empty)}"));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Engine.ActionReceived -= OnActionReceived;
        }
    }

    protected override void OnQueryReceived(ReportQueryReceivedEventArgs args)
    {
    }
}
