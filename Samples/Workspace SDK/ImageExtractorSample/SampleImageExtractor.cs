// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Sdk.Workspace.Components.ImageExtractor;

public sealed class SampleImageExtractor : ImageExtractor
{
    public override ImageSource Icon { get; } = new BitmapImage(new Uri("pack://application:,,,/ImageExtractorSample;Component/Resources/Icon.png", UriKind.RelativeOrAbsolute));

    public override string Name => "Load from vCard...";

    public override Guid UniqueId { get; } = new Guid("5EDBB0B6-8253-433E-99A1-9021E498437A");


    public override ImageSource GetImage()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "vCard files (*.vcf)|*.vcf|All files (*.*)|*.*",
            Title = "Open vCard File"
        };

        return openFileDialog.ShowDialog() == true ? VCardReader.ReadVCard(openFileDialog.FileName)?.Picture : null;
    }

    public override bool SupportsContext(ImageExtractorContext context)
    {
        return context == ImageExtractorContext.CardholderPicture;
    }
}