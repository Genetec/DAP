// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;

public partial class PasswordChangeWindow : Window
{
    public SecureString NewPassword { get; private set; }

    public PasswordChangeWindow(string previousError)
    {
        InitializeComponent();
        Error.Text = previousError ?? "";
        Loaded += (_, _) => NewPasswordInput.Focus();
        Closed += (_, _) =>
        {
            NewPasswordInput.Clear();
            ConfirmationInput.Clear();
        };
    }

    private void OnChangePasswordClicked(object sender, RoutedEventArgs e)
    {
        using var password = NewPasswordInput.SecurePassword;
        using var confirmation = ConfirmationInput.SecurePassword;

        if (password.Length == 0 || !PasswordsMatch(password, confirmation))
        {
            Error.Text = "Enter a new password and an identical confirmation.";
            ConfirmationInput.Clear();
            ConfirmationInput.Focus();
            return;
        }

        // The Directory enforces complexity and password-history rules.
        NewPassword = password.Copy();
        DialogResult = true;
    }

    internal static bool PasswordsMatch(SecureString first, SecureString second)
    {
        if (first.Length != second.Length)
            return false;

        IntPtr firstBuffer = IntPtr.Zero;
        IntPtr secondBuffer = IntPtr.Zero;
        try
        {
            firstBuffer = Marshal.SecureStringToGlobalAllocUnicode(first);
            secondBuffer = Marshal.SecureStringToGlobalAllocUnicode(second);

            int difference = 0;
            for (int i = 0; i < first.Length; i++)
            {
                difference |= Marshal.ReadInt16(firstBuffer, i * 2) ^ Marshal.ReadInt16(secondBuffer, i * 2);
            }

            return difference == 0;
        }
        finally
        {
            if (firstBuffer != IntPtr.Zero)
                Marshal.ZeroFreeGlobalAllocUnicode(firstBuffer);
            if (secondBuffer != IntPtr.Zero)
                Marshal.ZeroFreeGlobalAllocUnicode(secondBuffer);
        }
    }
}
