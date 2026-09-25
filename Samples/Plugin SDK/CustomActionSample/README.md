# Custom action sample

One Security Center plugin contributes two custom action types: an entity-aware SDK action and an outbound HTTP action. The actions use different configuration and execution paths.

## Included actions

| Action | What it demonstrates |
| --- | --- |
| **Launch encoder command** | Selecting a camera, loading choices from the selected entity, serializing entity-specific configuration, and calling a Platform SDK operation |
| **Send HTTP request** | Configuring an absolute URL, query parameters, headers, content type, and body, then performing asynchronous external I/O from the plugin role |

Both actions share the plugin role, server module, client module, and icon. Each action builder has its own SDK certificate.

## How custom actions work

The server plugin registers each action type with a stable GUID, display name, description, supported usage, and icon. The Workspace module registers a builder for each type in Config Tool and Security Desk.

When an operator configures an action, its view serializes the settings into the custom-action payload and selects the plugin role as the recipient. When Security Center triggers the action, the plugin receives `Engine.ActionReceived`, identifies the action GUID, deserializes the payload, and performs the operation.

## Configure the sample

Use a Windows development system with Security Center, its SDK, and the .NET Framework 4.8.1 targeting pack. Set `GSC_SDK` to the installed SDK directory containing the .NET Framework assemblies.

1. Build `CustomActionSample.csproj` from an elevated Visual Studio instance. The post-build target registers the client and server modules.
2. Restart Config Tool and Security Desk so they load the client module.
3. Create and activate the **Custom Action Sample** plugin role in Config Tool.
4. Create an event-to-action or another supported action configuration.
5. Select either **Launch encoder command** or **Send HTTP request**.

## Launch an encoder command

Select a camera, then select one of the encoder commands reported by that camera. The action is valid only when both values are selected.

When the action runs, the plugin resolves the camera entity and calls `LaunchEncoderCommand` with the configured command ID.

## Send an HTTP request

Select an HTTP method and enter an absolute HTTP or HTTPS URL. You can add query parameters and request headers, and supply a body and content type for POST, PUT, and PATCH requests.

The plugin appends configured query parameters to any query string already present in the URL. It reuses one `HttpClient`, applies a 30-second timeout, and writes the response status or failure to the plugin log.

## Explore the code

These files contain the action definitions, registrations, views, and execution logic:

- `Client/SampleCustomActionView.xaml` configures the encoder-command action.
- `Client/HttpRequestActionView.xaml` configures the HTTP action.
- `LaunchEncoderCommandAction.cs` and `SendHttpRequestAction.cs` define the serialized payloads.
- `Client/SampleModule.cs` registers both action builders.
- `Server/SamplePlugin.cs` registers and executes both action types.
