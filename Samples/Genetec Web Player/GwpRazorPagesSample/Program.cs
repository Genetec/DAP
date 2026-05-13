// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var mediaGateway = builder.Configuration.GetSection("MediaGateway");
var endpoint = mediaGateway["Endpoint"]?.TrimEnd('/') ?? throw new InvalidOperationException("MediaGateway:Endpoint is required.");
var username = mediaGateway["Username"] ?? throw new InvalidOperationException("MediaGateway:Username is required.");
var password = mediaGateway["Password"] ?? string.Empty;
var sdkCertificate = mediaGateway["SdkCertificate"] ?? throw new InvalidOperationException("MediaGateway:SdkCertificate is required.");
var serverVersion = mediaGateway["ServerVersion"];

var authorizationParameter = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username};{sdkCertificate}:{password}"));

builder.Services.AddRazorPages();

builder.Services.AddHttpClient("MediaGateway", client =>
{
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationParameter);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    return handler;
});

builder.Services.AddSingleton(new MediaGatewayOptions
{
    Endpoint = endpoint,
    ServerVersion = string.IsNullOrWhiteSpace(serverVersion) ? null : serverVersion.Trim(),
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
    context.Items["CspNonce"] = nonce;

    context.Response.Headers["Content-Security-Policy"] = string.Join("; ",
        "default-src 'self' https:",
        $"script-src 'self' https: 'nonce-{nonce}'",
        $"style-src 'self' 'nonce-{nonce}'",
        "connect-src 'self' https: wss:",
        "media-src https: blob:");

    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.MapGet("/api/token/{cameraId}", async (string cameraId, IHttpClientFactory httpClientFactory) =>
{
    if (string.IsNullOrWhiteSpace(cameraId))
    {
        return Results.BadRequest("A camera GUID is required.");
    }

    var client = httpClientFactory.CreateClient("MediaGateway");
    var response = await client.GetAsync($"{endpoint}/v2/token/{Uri.EscapeDataString(cameraId.Trim())}");

    if (!response.IsSuccessStatusCode)
    {
        return Results.StatusCode((int)response.StatusCode);
    }

    var token = await response.Content.ReadAsStringAsync();
    return Results.Text(token);
});

app.Run();

public class MediaGatewayOptions
{
    public required string Endpoint { get; init; }
    public string? ServerVersion { get; init; }
}
