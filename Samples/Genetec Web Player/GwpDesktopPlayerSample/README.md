# GWP Desktop Player Sample

This sample demonstrates the feasible hosting model for the Genetec Web Player inside a .NET WPF application:

- WPF hosts a `WebView2` control.
- The `WebView2` instance serves a local page from the virtual host `https://app.local`.
- The local page loads `gwp.js` directly from the target Media Gateway and runs GWP in the browser environment it expects.
- Token retrieval runs in .NET via a COM-visible `TokenProvider` exposed to JavaScript through `AddHostObjectToScript`.
- The hosted page receives non-secret bootstrap settings and requests opaque camera tokens from the native token provider. Media Gateway credentials stay in the WPF host process.

## Run

```powershell
dotnet run
```

### Native playback configuration

The WPF host loads Media Gateway settings from environment variables before it creates the WebView:

- `GWP_MEDIA_GATEWAY_ENDPOINT`
- `GWP_USERNAME`
- `GWP_PASSWORD`
- `GWP_SDK_CERTIFICATE`
- `GWP_SERVER_VERSION` (optional)

If you do not set them, the sample falls back to the local development defaults:

- Media Gateway endpoint: `https://localhost/media`
- Username: `admin`
- Password: blank
- SDK certificate: Genetec development certificate

After startup, the WPF header lets the operator edit the username, password, and SDK certificate natively. Those values stay in the host process and become the authentication inputs used by the .NET `TokenProvider` for later token requests.

Example:

```powershell
$env:GWP_MEDIA_GATEWAY_ENDPOINT = 'https://localhost/media'
$env:GWP_USERNAME = 'admin'
$env:GWP_PASSWORD = ''
$env:GWP_SDK_CERTIFICATE = 'KxsD11z743Hf5Gq9mv3+5ekxzemlCiUXkTFY5ba1NOGcLCmGstt2n0zYE9NsNimv'
$env:GWP_SERVER_VERSION = '5.12.0.0'
dotnet run
```

## Required environment setup

### 1. Trust the Media Gateway certificate

If the Media Gateway certificate is self-signed or otherwise untrusted, WebView2 will fail to load `gwp.js` or connect to the gateway.

For development (`Debug` builds only), this sample automatically allows certificate warnings for `localhost`, `127.0.0.1`, and `::1` inside both WebView2 and the .NET `TokenProvider`. These bypasses are compiled out in `Release` builds. Production deployments should use a trusted certificate.

### 2. Allow the hosted page origin in Media Gateway CORS

This sample uses the origin `https://app.local`.

If strict CORS is enabled, add that origin to `MediaGateway.gconfig`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Configuration>
    <MediaGateway EnforceStrictCrossOrigin="true">
        <AllowedOrigin Origin="https://app.local" />
    </MediaGateway>
</Configuration>
```

Restart the Media Gateway role after the change.

### 3. Use a matching GWP build

The sample loads `gwp.js` from `${mediaGatewayEndpoint}/v2/files/gwp.js` so the player version matches the Security Center version.

## Scope and limitations

- This sample demonstrates a feasible hosting pattern. It is not a production-ready security design.
- Media Gateway authentication is configured in the WPF host and token requests are executed by native `HttpClient`.
- Username, password, and SDK certificate can be edited in the WPF header at runtime. They never enter the browser page, but they remain operator-supplied authentication values that should be handled carefully in a real application.
- The default SDK certificate is the Genetec development certificate intended for SDK development only, until the operator overrides it in the WPF header.
- The effective trust boundary for this sample is the local `https://app.local` page plus the `gwp.js` file loaded from the configured Media Gateway. For production, move to a fully native authentication flow, for example through the Security Center SDK `Engine`, and consider how you want to version and trust the GWP asset itself.
- DevTools access and certificate bypass are gated behind `#if DEBUG` and are compiled out of `Release` builds.
- A Content Security Policy meta tag restricts script sources, connections, and media to `self`, `https:`, `wss:`, and `blob:`.
- The WPF host blocks top-level navigations away from the `app.local` virtual host.
- Player startup is cancellable. Clicking Stop during script load or session establishment cancels the in-flight start and cleans up any partially created player.
- WebView2 user data is stored under `%LOCALAPPDATA%\GwpDesktopPlayerSample\WebView2Data`.
- Browser autoplay rules apply to audio.
- Video rendering and overlays remain in the HTML layer, not the WPF visual tree.
