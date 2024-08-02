// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples.Client
{
    using System;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Genetec.Sdk.Workspace.Pages;
    using Genetec.Sdk.Workspace.Tasks;

    public class CustomPageDescriptor : PageDescriptor
    {
        public override bool AllowOfflineExecution => false;

        public override Guid CategoryId => new(TaskCategories.Investigation);

        public override string Description => "View custom events that occured on selected entities";

        public override ImageSource Icon { get; } = new BitmapImage(new Uri("pack://application:,,,/CustomReportSample;component/Resources/Images/SmallLogo.png"));

        public override ImageSource Thumbnail { get; } = new BitmapImage(new Uri("pack://application:,,,/CustomReportSample;component/Resources/Images/LargeLogo.png"));

        public override TaskIconColor IconColor => TaskIconColor.VideoIconColor;

        public override string Name => "Custom report";

        public override Guid Type => CustomReportId.Value;
    }
}