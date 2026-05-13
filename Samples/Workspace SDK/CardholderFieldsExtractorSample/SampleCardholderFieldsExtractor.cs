// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

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
        var dialog = new OpenFileDialog
        {
            Filter = "vCard files (*.vcf)|*.vcf|All files (*.*)|*.*",
            Title = "Open vCard File"
        };

        if (dialog.ShowDialog() != true)
            return null;

        VCard vCardInfo = VCardReader.ReadVCard(dialog.FileName);
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

        return null;
    }
}