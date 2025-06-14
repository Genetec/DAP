// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Sdk.Workspace.ContextualAction;

public class SampleContextualActionGroup : ContextualActionGroup
{
    public SampleContextualActionGroup()
    {
        Name = "Sample Contextual action group";
        Icon = CreateIcon();

        ImageSource CreateIcon()
        {
            var icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri("pack://application:,,,/ConfigPageSample;component/Resources/Images/SmallLogo.png");
            icon.DecodePixelWidth = 16;
            icon.DecodePixelHeight = 16;
            icon.EndInit();
            icon.Freeze();
            return icon;
        }
    }

    public override Guid Id => ContextualActionGroupId.SampleContextualActionGroupId;
}