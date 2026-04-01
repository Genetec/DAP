// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;

namespace Genetec.Dap.CodeSamples;

public partial class MainWindow : Window
{
    private static readonly Uri s_appUri = new("https://app.local/index.html");
    private static readonly HashSet<string> s_developmentHosts = ["localhost", "127.0.0.1", "::1"];

    private NativePlaybackConfiguration? m_playbackConfiguration;
    private TokenProvider? m_tokenProvider;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closed += OnClosed;
        PreviewKeyDown += OnPreviewKeyDown;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            m_playbackConfiguration = NativePlaybackConfiguration.LoadFromEnvironment();
            UsernameTextBox.Text = m_playbackConfiguration.Username;
            PasswordBox.Password = m_playbackConfiguration.Password;
            SdkCertificateTextBox.Text = m_playbackConfiguration.SdkCertificate;
            m_tokenProvider = new TokenProvider(m_playbackConfiguration);

            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GwpDesktopPlayerSample",
                "WebView2Data");
            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);

            await Browser.EnsureCoreWebView2Async(environment);
#if DEBUG
            Browser.CoreWebView2.ServerCertificateErrorDetected += OnServerCertificateErrorDetected;
#endif
            Browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app.local",
                webRoot,
                CoreWebView2HostResourceAccessKind.Allow);
            Browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
#if DEBUG
            Browser.CoreWebView2.Settings.AreDevToolsEnabled = true;
#else
            Browser.CoreWebView2.Settings.AreDevToolsEnabled = false;
#endif
            Browser.CoreWebView2.Settings.IsStatusBarEnabled = true;

            var bootstrapScript = $"window.__GWP_HOST_CONFIG__ = {JsonSerializer.Serialize(new
            {
                mediaGatewayEndpoint = m_playbackConfiguration.MediaGatewayEndpoint,
                serverVersion = m_playbackConfiguration.ServerVersion,
            })};";
            await Browser.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(bootstrapScript);

            Browser.CoreWebView2.AddHostObjectToScript("tokenProvider", m_tokenProvider);
            Browser.CoreWebView2.NavigationStarting += OnNavigationStarting;
            Browser.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
            Browser.CoreWebView2.Navigate(s_appUri.ToString());
            StatusTextBlock.Text =
                $"WebView2 ready. Native playback configuration loaded for {m_playbackConfiguration.MediaGatewayEndpoint}.";
        }
        catch (Exception exception)
        {
            StatusTextBlock.Text = "Failed to initialize WebView2.";
            MessageBox.Show(this, exception.ToString(), "WebView2 initialization failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri) && uri.Host != s_appUri.Host)
        {
            e.Cancel = true;
        }
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        StatusTextBlock.Text = e.IsSuccess
            ? "Local GWP host page loaded. Enter a camera GUID to start playback."
            : $"Navigation failed with error {e.WebErrorStatus}.";
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        (m_tokenProvider as IDisposable)?.Dispose();
        m_tokenProvider = null;
    }

    private void OnServerCertificateErrorDetected(object? sender, CoreWebView2ServerCertificateErrorDetectedEventArgs e)
    {
        if (!Uri.TryCreate(e.RequestUri, UriKind.Absolute, out var uri))
        {
            return;
        }

        if (!s_developmentHosts.Contains(uri.Host))
        {
            return;
        }

        e.Action = CoreWebView2ServerCertificateErrorAction.AlwaysAllow;
        StatusTextBlock.Text = $"Allowed certificate warning for development endpoint {uri.Host}.";
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        if (Browser.CoreWebView2 is null)
        {
            return;
        }

        Browser.Reload();
    }

    private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e) => SyncNativeCredentials();

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) => SyncNativeCredentials();

    private void SdkCertificateTextBox_TextChanged(object sender, TextChangedEventArgs e) => SyncNativeCredentials();

    private void DevToolsButton_Click(object sender, RoutedEventArgs e)
    {
#if DEBUG
        Browser.CoreWebView2?.OpenDevToolsWindow();
#endif
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
#if DEBUG
        if (e.Key != Key.F12)
        {
            return;
        }

        Browser.CoreWebView2?.OpenDevToolsWindow();
        e.Handled = true;
#endif
    }

    private void SyncNativeCredentials()
    {
        m_tokenProvider?.UpdateAuthentication(UsernameTextBox.Text, PasswordBox.Password, SdkCertificateTextBox.Text);
    }
}
