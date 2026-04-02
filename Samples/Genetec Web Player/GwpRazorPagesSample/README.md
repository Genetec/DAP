# GWP Razor Pages Sample

This sample demonstrates hosting the Genetec Web Player inside an ASP.NET Core Razor Pages application with production-ready CSP nonce support:

- ASP.NET Core serves a Razor Page that loads and runs GWP.
- The page is server-rendered, so the Media Gateway endpoint and server version are injected directly into the markup. No client-side configuration fetch is needed.
- A per-request cryptographic CSP nonce is generated in middleware, added to the `Content-Security-Policy` response header, and passed to each `<script>` and `<style>` tag via the Razor page model. This eliminates the need for `'unsafe-inline'`.
- A server-side `/api/token/{cameraId}` endpoint proxies token requests to the Media Gateway using credentials from `appsettings.json`. Media Gateway credentials never reach the browser.
- The page loads `gwp.js` directly from the target Media Gateway and uses the browser environment GWP expects.

Compared to the Minimal API sample, this sample adds:

- **CSP nonces** instead of `'unsafe-inline'`, demonstrating how to tighten the Content Security Policy for production use.
- **Server-rendered configuration** injected directly into the Razor markup, removing the need for a separate `/api/config` endpoint.

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

If strict CORS is enabled, add the ASP.NET application origin (the URL shown when you run the app, for example `https://localhost:5001`) to `MediaGateway.gconfig`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Configuration>
    <MediaGateway EnforceStrictCrossOrigin="true">
        <!-- Replace https://localhost:5001 with the actual ASP.NET app origin shown in the console or configured in applicationUrl -->
        <AllowedOrigin Origin="https://localhost:5001" />
    </MediaGateway>
</Configuration>
```

Restart the Media Gateway role after the change.

### 3. Use a matching GWP build

The sample loads `gwp.js` from `${mediaGatewayEndpoint}/v2/files/gwp.js` so the player version matches the Security Center version.

## CSP nonce implementation

The Content Security Policy is enforced via HTTP response header, not a meta tag. Each request receives a fresh cryptographic nonce:

1. **Middleware** in `Program.cs` generates a random nonce using `RandomNumberGenerator`, stores it in `HttpContext.Items`, and writes the `Content-Security-Policy` header.
2. **Page model** (`Index.cshtml.cs`) reads the nonce from `HttpContext.Items` and exposes it as a property.
3. **Razor markup** (`Index.cshtml`) applies the nonce to each `<script nonce="@nonce">` and `<style nonce="@nonce">` tag.

Because the nonce is unique per request and cryptographically random, inline scripts and styles are allowed only when they carry the correct nonce. An attacker who injects markup cannot predict the nonce value.

## Scope and limitations

- This sample demonstrates a feasible hosting pattern. It is not a production-ready security design.
- This sample has no user authentication. Anyone who can reach the application can view camera streams. A real application must add its own authentication layer to control who can access the application.
- Media Gateway credentials are stored in `appsettings.json`. For production, use a secure configuration provider such as user secrets, Azure Key Vault, or environment variables.
- The default SDK certificate is the Genetec development certificate intended for SDK development only.
- Player startup is cancellable. Clicking Stop during script load or session establishment cancels the in-flight start and cleans up any partially created player.
- Browser autoplay rules apply to audio.
- Video rendering and overlays remain in the HTML layer, not the ASP.NET server.
