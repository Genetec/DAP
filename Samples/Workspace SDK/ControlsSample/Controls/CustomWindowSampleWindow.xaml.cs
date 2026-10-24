// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System.Windows;
using Sdk.Controls;

public partial class CustomWindowSampleWindow : CustomWindow
{
    public CustomWindowSampleWindow(string displayName)
    {
        InitializeComponent();
        displayNameTextBox.Text = displayName;
    }

    public string DisplayName => displayNameTextBox.Text;

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
