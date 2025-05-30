﻿// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

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