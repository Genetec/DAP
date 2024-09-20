// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Windows.Media.Imaging;

public static class Icon
{
    private static BitmapImage LoadIcon(string resourceName)
    {
        var bitmap = new BitmapImage(new Uri(resourceName, UriKind.RelativeOrAbsolute));
        bitmap.Freeze();
        return bitmap;
    }

    public static BitmapImage SmallIcon => LoadIcon("pack://application:,,,/CustomEntitySample;component/Resources/SmallIcon.png");
    public static BitmapImage LargeIcon => LoadIcon("pack://application:,,,/CustomEntitySample;component/Resources/LargeIcon.png");
}