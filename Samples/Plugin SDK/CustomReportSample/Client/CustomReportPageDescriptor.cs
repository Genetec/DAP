// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Client;

using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Genetec.Sdk.Workspace.Pages;
using Genetec.Sdk.Workspace.Tasks;

public class CustomReportPageDescriptor : PageDescriptor
{
    // Do not allow offline execution
    public override bool AllowOfflineExecution => false;

    public override Guid CategoryId => new(TaskCategories.Investigation);

    public override string Description => "View custom events that occured on selected entities";

    public override ImageSource Icon { get; } = new BitmapImage(new Uri("pack://application:,,,/CustomReportSample;component/Resources/Images/SmallLogo.png"));

    public override ImageSource Thumbnail { get; } = new BitmapImage(new Uri("pack://application:,,,/CustomReportSample;component/Resources/Images/LargeLogo.png"));
    
    public override TaskIconColor IconColor => TaskIconColor.DefaultIconColor;

    public override string Name => "Custom report";

    // This is the identifier of the custom report
    public override Guid Type => CustomReportId.Value;

    // Ensure that the custom report page is only available to users with a specific privilege
    // This report does not require a specific privilege
    public override bool HasPrivilege()
    {
        return true;
    }
}
