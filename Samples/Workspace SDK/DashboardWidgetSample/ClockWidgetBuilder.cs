// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
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
}