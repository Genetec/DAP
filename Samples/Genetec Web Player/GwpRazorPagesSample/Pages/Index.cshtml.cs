// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Genetec.Dap.CodeSamples.Pages;

public class IndexModel : PageModel
{
    public string MediaGatewayEndpoint { get; private set; } = string.Empty;

    public string? ServerVersion { get; private set; }

    public string CspNonce { get; private set; } = string.Empty;

    private readonly MediaGatewayOptions m_options;

    public IndexModel(MediaGatewayOptions options)
    {
        m_options = options;
    }

    public void OnGet()
    {
        MediaGatewayEndpoint = m_options.Endpoint;
        ServerVersion = m_options.ServerVersion;
        CspNonce = HttpContext.Items["CspNonce"] as string ?? string.Empty;
    }
}
