// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Windows.Media.Imaging;
using Genetec.Sdk.Workspace.Components.Dashboard;
using Sdk.Workspace.Components;

public sealed class ClockWidgetBuilder : DashboardWidgetBuilder
{
    public static Guid ClockWidgetTypeId = new Guid("3E27799F-2FE1-44B6-B131-0695D98503DE");

    public override string Description => "Simple clock";

    public override string Name => "Clock";

    public override BitmapSource Thumbnail => new BitmapImage(new Uri("pack://application:,,,/DashboardWidgetSample;Component/Resources/AlarmClock.png", UriKind.RelativeOrAbsolute));

    public override Guid UniqueId => ClockWidgetTypeId;

    public override Guid Category => DashboardWidgetCategories.GeneralId;

    public override DashboardWidget CreateWidget(DashboardWidgetBuilderContext context)
    {
        return new ClockWidget();
    }

    public override bool IsSupported(DashboardWidgetBuilderContext context)
    {
        return context.Type == ClockWidgetTypeId;
    }
}