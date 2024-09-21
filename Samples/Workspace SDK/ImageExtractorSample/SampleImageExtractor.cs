// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

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

    public override Guid UniqueId { get; } = new("5EDBB0B6-8253-433E-99A1-9021E498437A");


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