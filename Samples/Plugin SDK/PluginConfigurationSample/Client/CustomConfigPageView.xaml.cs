// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Client;

using System.Windows;

public partial class CustomConfigPageView
{
    public CustomConfigPageView()
    {
        InitializeComponent();
    }

    private void UsernameBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is CustomConfigPage viewModel)
        {
            viewModel.UserName = UsernameBox.SecurePassword;
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is CustomConfigPage viewModel)
        {
            viewModel.Password = PasswordBox.SecurePassword;
        }
    }
}
