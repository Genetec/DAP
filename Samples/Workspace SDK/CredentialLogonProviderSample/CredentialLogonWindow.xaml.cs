// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.ComponentModel;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Sdk;
using Sdk.Workspace.Components.LogonProvider;

public partial class CredentialLogonWindow : Window
{
    private readonly Func<LogonInfo, CancellationToken, Task<ConnectionStateCode>> m_logOn;
    private readonly bool m_requirePassword;
    private readonly LogonSettings m_settings = new();
    private CancellationTokenSource m_attempt;
    private bool m_closeRequested;

    public CredentialLogonWindow(Func<LogonInfo, CancellationToken, Task<ConnectionStateCode>> logOn, bool requirePassword)
    {
        m_logOn = logOn ?? throw new ArgumentNullException(nameof(logOn));
        m_requirePassword = requirePassword;
        InitializeComponent();
        try
        {
            DirectoryBox.Text = m_settings.Directory;
            UsernameBox.Text = m_settings.Username;
        }
        catch (ConfigurationErrorsException)
        {
            DirectoryBox.Text = "localhost";
            StatusText.Text = "The previous connection settings could not be loaded. Enter your connection details.";
        }

        // Never let a saved password dismiss the host's idle-mode authentication prompt.
        SetConnecting(false);
        if (requirePassword)
            StatusText.Text = "Enter your password to resume the session. Saved credentials are disabled.";
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (m_requirePassword)
            PasswordBox.Focus();
        else
            DirectoryBox.Focus();
    }

    private void OnUseSavedChanged(object sender, RoutedEventArgs e)
    {
        if (ManualCredentials == null)
            return;
        SetConnecting(m_attempt != null);
        if (UseSavedBox.IsChecked == true)
        {
            SaveBox.IsChecked = false;
            PasswordBox.Clear();
        }
    }

    private async void OnConnect(object sender, RoutedEventArgs e)
    {
        if (m_attempt != null)
            return;

        LogonInfo info = null;
        try
        {
            var directory = DirectoryBox.Text.Trim();
            CredentialStore.GetTarget(directory); // Validate before accessing storage or starting a connection.
            var useSaved = UseSavedBox.IsChecked == true && !m_requirePassword;
            var save = SaveBox.IsChecked == true && !m_requirePassword && !useSaved;
            string username;
            string password;
            if (useSaved)
            {
                var credential = CredentialStore.Read(directory);
                if (credential == null)
                {
                    StatusText.Text = "No saved credential exists for this Directory. Clear Use saved credential and enter your credentials.";
                    return;
                }
                username = credential.UserName;
                password = credential.Password;
            }
            else
            {
                username = UsernameBox.Text.Trim();
                password = PasswordBox.Password;
            }
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Enter a username.");

            info = new LogonInfo(directory, username, password) { RetryCount = 1 };
            password = null;
            PasswordBox.Clear();
            m_attempt = new CancellationTokenSource();
            SetConnecting(true);
            StatusText.Text = "Connecting...";

            var result = await m_logOn(info, m_attempt.Token);
            if (result != ConnectionStateCode.Success)
            {
                StatusText.Text = result == ConnectionStateCode.LogonAborted ? "Logon canceled." :
                    $"Logon failed: {result}. Check your credentials and Directory. Saved credentials have not been changed.";
                return;
            }

            // Updating storage is separate from authentication; a storage failure does not undo logon.
            try
            {
                if (save)
                    CredentialStore.Write(directory, username, info.Password);
                m_settings.Directory = directory;
                m_settings.Username = username;
                m_settings.Save();
            }
            catch (Exception ex) when (ex is Win32Exception or ConfigurationErrorsException or System.IO.IOException or UnauthorizedAccessException or ArgumentException)
            {
                MessageBox.Show(this, "Logon succeeded, but saving the credential or connection settings failed. " + ex.Message,
                    Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            m_closeRequested = true;
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Logon canceled.";
        }
        catch (Exception ex)
        {
            // Do not log the LogonInfo, credential object, or password.
            StatusText.Text = "Unable to log on: " + ex.Message;
        }
        finally
        {
            info = null;
            PasswordBox.Clear();
            m_attempt?.Dispose();
            m_attempt = null;
            SetConnecting(false);
            if (m_closeRequested)
                Close();
        }
    }

    private void SetConnecting(bool connecting)
    {
        DirectoryBox.IsEnabled = ConnectButton.IsEnabled = !connecting;
        UseSavedBox.IsEnabled = ForgetButton.IsEnabled = !connecting && !m_requirePassword;
        ManualCredentials.IsEnabled = !connecting && UseSavedBox.IsChecked != true;
        SaveBox.IsEnabled = !connecting && !m_requirePassword && UseSavedBox.IsChecked != true;
    }

    private void OnForget(object sender, RoutedEventArgs e)
    {
        try
        {
            var directory = DirectoryBox.Text.Trim();
            CredentialStore.GetTarget(directory);
            if (MessageBox.Show(this, "Forget the saved credential for " + directory + "?", Title,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            StatusText.Text = CredentialStore.Delete(directory) ? "Saved credential deleted." : "No saved credential exists for this Directory.";
            UseSavedBox.IsChecked = false;
        }
        catch (Exception ex)
        {
            StatusText.Text = "Unable to forget the credential: " + ex.Message;
        }
    }

    private void OnCancel(object sender, RoutedEventArgs e) => RequestClose();

    public void RequestClose()
    {
        m_closeRequested = true;
        if (m_attempt == null)
            Close();
        else
        {
            StatusText.Text = "Canceling logon...";
            m_attempt.Cancel();
        }
    }

    private void OnClosing(object sender, CancelEventArgs e)
    {
        PasswordBox.Clear();
        if (m_attempt != null)
        {
            e.Cancel = true;
            RequestClose();
        }
    }

    private sealed class LogonSettings : ApplicationSettingsBase
    {
        [UserScopedSetting, DefaultSettingValue("localhost")]
        public string Directory { get => (string)this[nameof(Directory)]; set => this[nameof(Directory)] = value; }

        [UserScopedSetting, DefaultSettingValue("")]
        public string Username { get => (string)this[nameof(Username)]; set => this[nameof(Username)] = value; }
    }
}
