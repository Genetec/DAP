// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Genetec.Dap.CodeSamples;

internal sealed class NativePlaybackConfiguration
{
    private const string DefaultMediaGatewayEndpoint = "https://localhost/media";
    private const string DefaultUsername = "admin";
    private const string DefaultSdkCertificate = "KxsD11z743Hf5Gq9mv3+5ekxzemlCiUXkTFY5ba1NOGcLCmGstt2n0zYE9NsNimv";

    public string MediaGatewayEndpoint { get; }

    public string Username { get; }

    public string Password { get; }

    public string SdkCertificate { get; }

    public string? ServerVersion { get; }

    private NativePlaybackConfiguration(
        string mediaGatewayEndpoint,
        string username,
        string password,
        string sdkCertificate,
        string? serverVersion)
    {
        MediaGatewayEndpoint = NormalizeEndpoint(mediaGatewayEndpoint);
        Username = RequireValue(username, nameof(username));
        Password = password;
        SdkCertificate = RequireValue(sdkCertificate, nameof(sdkCertificate));
        ServerVersion = string.IsNullOrWhiteSpace(serverVersion) ? null : serverVersion.Trim();
    }

    public static NativePlaybackConfiguration LoadFromEnvironment() =>
        new(
            mediaGatewayEndpoint: ReadValue("GWP_MEDIA_GATEWAY_ENDPOINT", DefaultMediaGatewayEndpoint),
            username: ReadValue("GWP_USERNAME", DefaultUsername),
            password: Environment.GetEnvironmentVariable("GWP_PASSWORD") ?? string.Empty,
            sdkCertificate: ReadValue("GWP_SDK_CERTIFICATE", DefaultSdkCertificate),
            serverVersion: Environment.GetEnvironmentVariable("GWP_SERVER_VERSION"));

    private static string NormalizeEndpoint(string mediaGatewayEndpoint)
    {
        var candidate = RequireValue(mediaGatewayEndpoint, nameof(mediaGatewayEndpoint)).TrimEnd('/');
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException(
                $"The configured Media Gateway endpoint '{candidate}' is not a valid absolute URI.");
        }

        return candidate;
    }

    private static string ReadValue(string environmentVariableName, string fallbackValue)
    {
        var candidate = Environment.GetEnvironmentVariable(environmentVariableName);
        return string.IsNullOrWhiteSpace(candidate) ? fallbackValue : candidate.Trim();
    }

    private static string RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"The configured value for '{parameterName}' cannot be empty.");
        }

        return value.Trim();
    }
}

[ClassInterface(ClassInterfaceType.AutoDual)]
[ComVisible(true)]
public sealed class TokenProvider : IDisposable
{
    private static readonly HashSet<string> s_developmentHosts = ["localhost", "127.0.0.1", "::1"];

    private readonly object m_credentialSync = new();
    private readonly NativePlaybackConfiguration m_configuration;
    private readonly HttpClient m_httpClient;
    private bool m_hasUsername;
    private bool m_hasSdkCertificate;
    private string m_authorizationParameter = string.Empty;

    internal TokenProvider(NativePlaybackConfiguration configuration)
    {
        m_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        var handler = new HttpClientHandler();
#if DEBUG
        if (UsesDevelopmentCertificateBypass(m_configuration.MediaGatewayEndpoint))
        {
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }
#endif
        m_httpClient = new HttpClient(handler);
        UpdateAuthentication(m_configuration.Username, m_configuration.Password, m_configuration.SdkCertificate);
    }

    internal void UpdateAuthentication(string username, string password, string sdkCertificate)
    {
        var normalizedUsername = username?.Trim() ?? string.Empty;
        var normalizedPassword = password ?? string.Empty;
        var normalizedSdkCertificate = sdkCertificate?.Trim() ?? string.Empty;
        var authorizationParameter = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{normalizedUsername};{normalizedSdkCertificate}:{normalizedPassword}"));

        lock (m_credentialSync)
        {
            m_hasUsername = normalizedUsername.Length > 0;
            m_hasSdkCertificate = normalizedSdkCertificate.Length > 0;
            m_authorizationParameter = authorizationParameter;
        }
    }

    public async Task<string> GetToken(string cameraId)
    {
        if (string.IsNullOrWhiteSpace(cameraId))
        {
            throw new ArgumentException("A camera GUID is required.", nameof(cameraId));
        }

        string authorizationParameter;
        bool hasUsername;
        bool hasSdkCertificate;
        lock (m_credentialSync)
        {
            hasUsername = m_hasUsername;
            hasSdkCertificate = m_hasSdkCertificate;
            authorizationParameter = m_authorizationParameter;
        }

        if (!hasUsername)
        {
            throw new InvalidOperationException("A native username is required before requesting a token.");
        }

        if (!hasSdkCertificate)
        {
            throw new InvalidOperationException("A native SDK certificate is required before requesting a token.");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{m_configuration.MediaGatewayEndpoint}/v2/token/{Uri.EscapeDataString(cameraId.Trim())}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authorizationParameter);

        var response = await m_httpClient.SendAsync(request).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

#if DEBUG
    private static bool UsesDevelopmentCertificateBypass(string mediaGatewayEndpoint) =>
        Uri.TryCreate(mediaGatewayEndpoint, UriKind.Absolute, out var uri) && s_developmentHosts.Contains(uri.Host);
#endif

    void IDisposable.Dispose() => m_httpClient.Dispose();
}
