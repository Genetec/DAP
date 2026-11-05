// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Genetec.Sdk.EventsArgs;
using System;
using System.Collections.Generic;
using System.Windows;

public partial class CertificateWindow : Window
{
    public CertificateWindow(DirectoryCertificateValidationEventArgs validation)
    {
        InitializeComponent();

        var certificate = validation.DirectoryCertificate;
        var warnings = new List<string>();
        if (validation.CertificateRevoked)
            warnings.Add("The certificate is revoked. This sample refuses revoked certificates.");

        if (validation.CertificateExpired)
            warnings.Add("The certificate is expired or outside its validity period.");

        if (validation.CertificateIsUntrustedRoot)
            warnings.Add("The certificate issuer is not trusted.");

        if (validation.CertificateDnsMismatch)
            warnings.Add("The certificate name does not match the Directory hostname.");

        if (warnings.Count == 0)
            warnings.Add("The Directory certificate requires confirmation.");

        Warnings.Text = string.Join(Environment.NewLine, warnings);
        TrustButton.IsEnabled = certificate != null && !validation.CertificateRevoked;
        Details.Text = $"Directory: {validation.DirectoryHostName}{Environment.NewLine}"
            + $"Endpoint: {validation.DirectoryEndPoint}{Environment.NewLine}";

        if (certificate != null)
        {
            Details.AppendText($"Subject: {certificate.Subject}{Environment.NewLine}"
                + $"Issuer: {certificate.Issuer}{Environment.NewLine}"
                + $"Valid from: {certificate.NotBefore:u}{Environment.NewLine}"
                + $"Valid until: {certificate.NotAfter:u}{Environment.NewLine}"
                + $"Thumbprint: {certificate.Thumbprint}");
        }
    }

    private void OnTrustClicked(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
