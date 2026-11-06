// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Genetec.Sdk;
using Genetec.Sdk.EventsArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

public partial class LoginWindow : Window
{
    private readonly List<SecureString> m_sessionPasswords = new();
    private Engine m_engine;
    private Action m_unsubscribe;
    private CancellationTokenSource m_attemptCancellation;
    private Task m_pendingOperation = Task.CompletedTask;
    private Task m_pendingCancellation = Task.CompletedTask;
    private Window m_activePrompt;
    private bool m_cancellationRequested;
    private bool m_isBusy;
    private bool m_isClosing;
    private bool m_isDisconnecting;
    private bool m_canClose;
    private string m_logonFailureMessage;

    public LoginWindow() => InitializeComponent();

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        UsernameInput.Focus();
    }

    private void OnWindowsCredentialsChanged(object sender, RoutedEventArgs e)
    {
        UsernameInput.IsEnabled = WindowsCredentials.IsChecked != true;
        PasswordInput.IsEnabled = WindowsCredentials.IsChecked != true;
        PasswordInput.Clear();
    }

    private async void OnLogOnClicked(object sender, RoutedEventArgs e)
    {
        if (m_isBusy || m_isClosing)
            return;

        m_pendingOperation = LogOnAsync();
        await m_pendingOperation;
    }

    private async Task LogOnAsync()
    {
        string directory = DirectoryInput.Text.Trim();
        string username = UsernameInput.Text.Trim();
        bool useWindowsCredentials = WindowsCredentials.IsChecked == true;
        bool useSupervisor = SupervisorSection.IsExpanded;
        string supervisor = SupervisorInput.Text.Trim();
        Error.Text = "";
        if (directory.Length == 0
            || (!useWindowsCredentials && username.Length == 0)
            || (useSupervisor && supervisor.Length == 0))
        {
            Error.Text = "Enter a Directory and username, including the supervisor username when supervisor logon is expanded.";
            return;
        }

        // The SDK retains these objects for reconnects, so release them with the Engine.
        SecureString password = useWindowsCredentials ? null : PasswordInput.SecurePassword;
        SecureString supervisorPassword = useSupervisor ? SupervisorPassword.SecurePassword : null;
        if (password != null)
            m_sessionPasswords.Add(password);
        if (supervisorPassword != null)
            m_sessionPasswords.Add(supervisorPassword);

        using var cancellation = new CancellationTokenSource();
        m_attemptCancellation = cancellation;
        m_pendingCancellation = Task.CompletedTask;
        m_cancellationRequested = false;
        m_logonFailureMessage = null;
        SetBusy(true);
        Status.Text = "Connecting...";
        SetTrust("Directory not verified", Brushes.Silver);
        bool keepConnection = false;

        try
        {
            m_engine = new Engine();
            SubscribeToLoginEvents();

            var login = m_engine.LoginManager;
            ConnectionStateCode result;
            if (useWindowsCredentials && useSupervisor)
            {
                result = await login.LogOnUsingWindowsCredentialAsync(
                    directory, supervisor, supervisorPassword, cancellation.Token);
            }
            else if (useWindowsCredentials)
            {
                result = await login.LogOnUsingWindowsCredentialAsync(directory, cancellation.Token);
            }
            else if (useSupervisor)
            {
                result = await login.LogOnAsync(
                    directory, username, password, supervisor, supervisorPassword, cancellation.Token);
            }
            else
            {
                result = await login.LogOnAsync(directory, username, password, cancellation.Token);
            }

            // A successful result racing with cancellation must not leave an unwanted session open.
            if (m_cancellationRequested || m_isClosing || result == ConnectionStateCode.LogonAborted)
            {
                Status.Text = "Logon cancelled.";
                return;
            }

            if (result != ConnectionStateCode.Success)
            {
                Status.Text = "Logon failed.";
                Error.Text = m_logonFailureMessage ?? result.ToString();
                return;
            }

            keepConnection = true;
            LoginPanel.Visibility = Visibility.Collapsed;
            ConnectedPanel.Visibility = Visibility.Visible;
            ConnectionHeading.Text = "Connected";
            string displayUsername = useWindowsCredentials
                ? $"{Environment.UserDomainName}\\{Environment.UserName}"
                : username;

            ConnectedDirectory.Text = $"Directory: {login.ActualDirectoryEndPoint}";
            ConnectedUser.Text = $"User: {displayUsername}";
            Status.Text = "Logged on.";
            SetTrust("Trusted Directory", Brushes.LimeGreen);
        }
        catch (Exception ex)
        {
            Status.Text = m_cancellationRequested ? "Logon cancelled." : "Logon failed.";
            if (!m_cancellationRequested)
                Error.Text = ex.Message;
        }
        finally
        {
            // Cancel can execute SDK callbacks synchronously. Join it before disposing its source.
            try
            {
                await m_pendingCancellation;
            }
            catch (Exception ex)
            {
                Error.Text = "Cancellation failed: " + ex.Message;
            }

            m_attemptCancellation = null;
            PasswordInput.Clear();
            SupervisorPassword.Clear();
            if (!keepConnection)
            {
                await ReleaseEngineAsync();
                SetTrust("Directory not verified", Brushes.Silver);
            }

            SetBusy(false);
        }
    }

    private void SubscribeToLoginEvents()
    {
        var engine = m_engine;
        var login = engine.LoginManager;

        // The SDK event sender is not necessarily the public LoginManager wrapper.
        // Capture the Engine so callbacks from a previous connection can still be ignored.
        void StatusChanged(object sender, LogonStatusChangedEventArgs e) => OnLogonStatusChanged(engine, e);
        void Failed(object sender, LogonFailedEventArgs e) => OnLogonFailed(engine, e);
        void LoggedOn(object sender, LoggedOnEventArgs e) => OnLoggedOn(engine, e);
        void LoggedOff(object sender, LoggedOffEventArgs e) => OnLoggedOff(engine, e);
        void CertificateValidation(object sender, DirectoryCertificateValidationEventArgs e)
        {
            OnCertificateValidation(engine, e);
        }

        void PasswordChange(object sender, RequestUserChangePasswordEventArg e) => OnPasswordChange(engine, e);

        m_unsubscribe = () =>
        {
            login.LogonStatusChanged -= StatusChanged;
            login.LogonFailed -= Failed;
            login.LoggedOn -= LoggedOn;
            login.LoggedOff -= LoggedOff;
            login.RequestDirectoryCertificateValidation -= CertificateValidation;
            login.RequestUserChangePassword -= PasswordChange;
        };
        login.LogonStatusChanged += StatusChanged;
        login.LogonFailed += Failed;
        login.LoggedOn += LoggedOn;
        login.LoggedOff += LoggedOff;
        login.RequestDirectoryCertificateValidation += CertificateValidation;
        login.RequestUserChangePassword += PasswordChange;
    }

    private void OnLogonStatusChanged(Engine engine, LogonStatusChangedEventArgs e)
    {
        PostToUi(engine, () =>
        {
            AppendConnectionEvent($"LogonStatusChanged: {e.Status}");
            if (!m_cancellationRequested)
                Status.Text = e.Status.ToString();
        });
    }

    private void OnLogonFailed(Engine engine, LogonFailedEventArgs e)
    {
        if (!ReferenceEquals(engine, m_engine))
            return;

        // The task continuation can run before a queued UI update, so retain the failure immediately.
        m_logonFailureMessage = e.FormattedErrorMessage;
        PostToUi(engine, () =>
        {
            AppendConnectionEvent("LogonFailed: " + e.FailureCode);
            if (m_attemptCancellation == null)
            {
                Status.Text = "Connection failed.";
                Error.Text = e.FormattedErrorMessage;
            }
        });
    }

    private void OnLoggedOn(Engine engine, LoggedOnEventArgs e)
    {
        PostToUi(engine, () =>
        {
            AppendConnectionEvent("LoggedOn");
            ConnectionHeading.Text = "Connected";
            ConnectedUser.Text = $"User: {e.UserName}";
            ConnectedDirectory.Text = $"Directory: {engine.LoginManager.ActualDirectoryEndPoint}";
            SetTrust("Trusted Directory", Brushes.LimeGreen);
            Status.Text = "Logged on.";
        });
    }

    private void OnLoggedOff(Engine engine, LoggedOffEventArgs e)
    {
        PostToUi(engine, () =>
        {
            AppendConnectionEvent($"LoggedOff; AutoReconnect={e.AutoReconnect}");
            SetTrust("Directory not verified", Brushes.Silver);
            if (e.AutoReconnect)
            {
                ConnectionHeading.Text = "Reconnecting...";
                Status.Text = "Connection lost. The SDK is reconnecting...";
            }
            else if (!m_isDisconnecting)
            {
                m_pendingOperation = LogOffAsync();
            }
        });
    }

    private void OnCertificateValidation(Engine engine, DirectoryCertificateValidationEventArgs e)
    {
        e.AcceptDirectory = false;
        HandleLoginPrompt(engine, () =>
        {
            SetTrust("Certificate confirmation required", Brushes.Gold);
            AppendConnectionEvent("RequestDirectoryCertificateValidation");
            var dialog = new CertificateWindow(e);
            if (ShowPrompt(dialog))
                e.AcceptDirectory = true;
            else
                RequestCancellation();
        });
    }

    private void OnPasswordChange(Engine engine, RequestUserChangePasswordEventArg e)
    {
        e.Cancel = true;
        HandleLoginPrompt(engine, () =>
        {
            AppendConnectionEvent("RequestUserChangePassword");
            var dialog = new PasswordChangeWindow(e.LastException?.Message);
            if (ShowPrompt(dialog))
            {
                // A changed password can also be reused by the SDK during reconnects.
                m_sessionPasswords.Add(dialog.NewPassword);
                e.SecureNewPassword = dialog.NewPassword;
                e.Cancel = false;
            }
            else
            {
                dialog.NewPassword?.Dispose();
                // Also cancel the attempt: SDK versions differ in how they honor the Cancel property.
                RequestCancellation();
            }
        });
    }

    private void HandleLoginPrompt(Engine engine, Action show)
    {
        try
        {
            // These are synchronous SDK decision callbacks. Return only after the modal dialog closes.
            Dispatcher.Invoke(() =>
            {
                if (!m_isClosing && !m_cancellationRequested
                    && ReferenceEquals(engine, m_engine))
                {
                    show();
                }
            });
        }
        catch (Exception ex) when (ex is TaskCanceledException || ex is InvalidOperationException)
        {
            // Closing the dispatcher leaves the event's default refusal/cancellation in place.
        }
    }

    private bool ShowPrompt(Window dialog)
    {
        dialog.Owner = this;
        m_activePrompt = dialog;
        try
        {
            return dialog.ShowDialog() == true && !m_cancellationRequested && !m_isClosing;
        }
        finally
        {
            m_activePrompt = null;
        }
    }

    private void PostToUi(Engine engine, Action update)
    {
        if (Dispatcher.HasShutdownStarted)
            return;

        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (!m_isClosing && ReferenceEquals(engine, m_engine))
                update();
        }));
    }

    private void AppendConnectionEvent(string message)
    {
        // Bound the trace while the sample remains connected for a long time.
        if (EventTrace.LineCount > 200)
            EventTrace.Clear();

        EventTrace.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  " + message + Environment.NewLine);
        EventTrace.ScrollToEnd();
    }

    private void SetTrust(string text, Brush brush)
    {
        TrustStatus.Text = text;
        TrustStatus.Foreground = brush;
    }

    private void SetBusy(bool busy)
    {
        m_isBusy = busy || m_isDisconnecting;
        Inputs.IsEnabled = !m_isBusy;
        LogOnButton.IsEnabled = !m_isBusy;
        LogOffButton.IsEnabled = !m_isBusy;
        CancelButton.IsEnabled = !m_isClosing;
        Progress.Visibility = m_isBusy ? Visibility.Visible : Visibility.Collapsed;
    }

    private void RequestCancellation()
    {
        if (m_cancellationRequested)
            return;

        m_cancellationRequested = true;
        Status.Text = "Cancelling...";
        CancelButton.IsEnabled = false;
        m_activePrompt?.Close();
        if (m_attemptCancellation != null)
        {
            var source = m_attemptCancellation;
            // Do not run synchronous SDK cancellation callbacks on the WPF dispatcher.
            m_pendingCancellation = Task.Run(() => source.Cancel());
        }
        else if (m_engine != null && !m_isDisconnecting)
        {
            // Automatic reconnect does not use the initial attempt's cancellation token.
            m_pendingOperation = LogOffAsync();
        }
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        if (m_attemptCancellation != null)
            RequestCancellation();
        else
            Close();
    }

    private async void OnLogOffClicked(object sender, RoutedEventArgs e)
    {
        if (m_isBusy || m_isClosing)
            return;

        m_pendingOperation = LogOffAsync();
        await m_pendingOperation;
    }

    private async Task LogOffAsync()
    {
        var pendingOperation = m_pendingOperation;
        m_isDisconnecting = true;
        SetBusy(true);
        RequestCancellation();

        // An initial login can still be finishing when a permanent disconnect arrives.
        await pendingOperation;

        AppendConnectionEvent("Logoff requested");
        bool released = await ReleaseEngineAsync();
        AppendConnectionEvent(released ? "Engine disposed" : "Connection cleanup failed");
        ConnectedPanel.Visibility = Visibility.Collapsed;
        LoginPanel.Visibility = Visibility.Visible;
        Status.Text = released ? "Logged off." : "Connection closed with a cleanup error.";
        SetTrust("Directory not verified", Brushes.Silver);
        m_isDisconnecting = false;
        SetBusy(false);
    }

    private async Task<bool> ReleaseEngineAsync()
    {
        var engine = m_engine;
        if (engine == null && m_sessionPasswords.Count == 0)
            return true;

        m_engine = null;
        m_unsubscribe?.Invoke();
        m_unsubscribe = null;
        var passwords = m_sessionPasswords.ToArray();
        m_sessionPasswords.Clear();
        try
        {
            await Task.Run(() =>
            {
                try
                {
                    // LogOff also cancels a reconnect when IsConnected is false.
                    engine?.LoginManager.LogOff();
                }
                finally
                {
                    try
                    {
                        engine?.Dispose();
                    }
                    finally
                    {
                        foreach (var password in passwords)
                            password.Dispose();
                    }
                }
            });
            return true;
        }
        catch (Exception ex)
        {
            Error.Text = "Connection cleanup failed: " + ex.Message;
            return false;
        }
    }

    private async void OnWindowClosing(object sender, CancelEventArgs e)
    {
        if (m_canClose)
            return;

        e.Cancel = true;
        if (m_isClosing)
            return;

        m_isClosing = true;
        RequestCancellation();
        m_activePrompt?.Close();
        await m_pendingOperation;
        await ReleaseEngineAsync();
        m_canClose = true;
        // Avoid recursively closing from within the initial synchronous Closing event.
        _ = Dispatcher.BeginInvoke(new Action(Close));
    }
}
