// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Windows.Documents;
    using Sdk.Credentials;
    using Sdk.Entities;
    using Sdk.Workspace.Components.BadgePrinter;

    internal class SampleBadgePrinter : BadgePrinter
    {
        public override string Name => "Badge Printer Sample";

        public override Guid UniqueId { get; } = new Guid("{9D4B5E78-9DE7-40EA-86C2-E70078DDC8D1}");

        public override bool Print(Guid credential, Guid badge, Guid cardholderGuid)
        {
            var credentialEntity = (Credential)Workspace.Sdk.GetEntity(credential);
            var badgeTemplate = (BadgeTemplate)Workspace.Sdk.GetEntity(badge);
            var cardholder = (Cardholder)Workspace.Sdk.GetEntity(cardholderGuid);

            return base.Print(credential, badge, cardholderGuid);
        }

        public override bool PrintAndEncode(BadgePrinterEncodingData data)
        {
            CredentialFormat credentialFormat = data.Credential;
            FixedDocument document = data.Document;
            var cardholder = (Cardholder)Workspace.Sdk.GetEntity(data.CardholderGuid);

            return base.PrintAndEncode(data);
        }

        public override BadgePrinterEnrollResult PrintAndEnroll(BadgePrinterPrintData data)
        {
            FixedDocument document = data.Document;
            var cardholder = (Cardholder)Workspace.Sdk.GetEntity(data.CardholderGuid);

            return base.PrintAndEnroll(data);
        }
    }
}
