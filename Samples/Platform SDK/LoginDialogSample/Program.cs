// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Runtime.CompilerServices;
using System.Windows;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        try
        {
            SdkResolver.Initialize();
            Run();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Login dialog sample", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Keep SDK-dependent types out of the entry point until the resolver is installed.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Run()
    {
        var application = new Application();
        application.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/LoginDialogSample;component/Theme.xaml", UriKind.Relative)
        });
        application.Run(new LoginWindow());
    }
}
