# GWP Minimal API Sample

This sample demonstrates hosting the Genetec Web Player inside an ASP.NET Core Minimal API application:

- ASP.NET Core serves a static HTML page that loads and runs GWP.
- A server-side `/api/token/{cameraId}` endpoint proxies token requests to the Media Gateway using credentials from `appsettings.json`. Media Gateway credentials never reach the browser.
- A `/api/config` endpoint provides the Media Gateway endpoint and server version to the page without exposing authentication details.
- The page loads `gwp.js` directly from the target Media Gateway and uses the browser environment GWP expects.

## Why this shape

GWP is a browser-oriented JavaScript library. It depends on DOM containers, HTML video and audio elements, canvas, WebSockets, and Media Source Extensions. ASP.NET Core serves the page and handles authentication server-side, while the browser provides the media runtime GWP requires.

This is the simplest possible web hosting model for GWP: one `Program.cs`, one static HTML file, and one configuration file.

## Run

```powershell
dotnet run
```

Then open the URL shown in the console output (for example, `https://localhost:5001`).

### Configuration

Media Gateway settings are configured in `appsettings.json`:

```json
{
  "MediaGateway": {
    "Endpoint": "https://localhost/media",
    "Username": "admin",
    "Password": "",
    "SdkCertificate": "KxsD11z743Hf5Gq9mv3+5ekxzemlCiUXkTFY5ba1NOGcLCmGstt2n0zYE9NsNimv",
    "ServerVersion": ""
  }
}
```

You can also use environment variables or user secrets:

```powershell
dotnet user-secrets set "MediaGateway:Password" "your-password"
```

## Required environment setup

### 1. Trust the Media Gateway certificate

If the Media Gateway certificate is self-signed or otherwise untrusted, the browser will fail to load `gwp.js` or connect to the gateway.

For development, the sample automatically allows certificate warnings when connecting to the Media Gateway from the server-side token endpoint. The browser must still trust the certificate for the `gwp.js` script load and WebSocket connections. Add the certificate to the browser's trust store or use a trusted certificate.

### 2. Allow the page origin in Media Gateway CORS

If strict CORS is enabled, add the ASP.NET application origin to `MediaGateway.gconfig`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Configuration>
    <MediaGateway EnforceStrictCrossOrigin="true">
        <AllowedOrigin Origin="https://localhost:5001" />
    </MediaGateway>
</Configuration>
```

Restart the Media Gateway role after the change.

### 3. Use a matching GWP build

The sample loads `gwp.js` from `${mediaGatewayEndpoint}/v2/files/gwp.js` so the player version matches the Security Center version.

## Scope and limitations

- This sample demonstrates a feasible hosting pattern. It is not a production-ready security design.
- The `/api/token/{cameraId}` endpoint has no authentication. Any client that can reach the server can request camera tokens. A real application must add its own user authentication layer in front of this endpoint.
- Media Gateway credentials are stored in `appsettings.json`. For production, use a secure configuration provider such as user secrets, Azure Key Vault, or environment variables.
- The default SDK certificate is the Genetec development certificate intended for SDK development only.
- A Content Security Policy meta tag restricts script sources, connections, and media to `self`, `https:`, `wss:`, and `blob:`. The policy allows `'unsafe-inline'` for scripts and styles because the page uses inline markup. The Razor Pages sample demonstrates how to use CSP nonces instead.
- Player startup is cancellable. Clicking Stop during script load or session establishment cancels the in-flight start and cleans up any partially created player.
- Browser autoplay rules apply to audio.
- Video rendering and overlays remain in the HTML layer, not the ASP.NET server.
