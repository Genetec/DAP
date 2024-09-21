// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Linq;
using Microsoft.Win32;
using Sdk.Workspace.Components.CardholderFieldsExtractor;

public class SampleCardholderFieldsExtractor : CardholderFieldsExtractor
{
    public override string Name => "Read vCard...";

    public override Guid UniqueId { get; } = new("88F33E43-1E51-4504-95BF-ADD2FBCBA8AD");

    public override CardholderFields GetFields(CardholderFieldsExtractorData data)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "vCard files (*.vcf)|*.vcf|All files (*.*)|*.*",
            Title = "Open vCard File"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            VCard vCardInfo = VCardReader.ReadVCard(openFileDialog.FileName);
            if (vCardInfo != null)
            {
                return new CardholderFields
                {
                    FirstName = vCardInfo.FirstName,
                    LastName = vCardInfo.LastName,
                    Email = vCardInfo.Emails.FirstOrDefault(),
                    Picture = vCardInfo.Picture,
                    Description = vCardInfo.Note
                };
            }
        }

        return null;
    }
}