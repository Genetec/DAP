// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Windows.Controls;
using Genetec.Sdk.Workspace.Services;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Workspace.Components.BadgePrinter;

class SampleBadgePrinter : BadgePrinter
{
    public override string Name => "Badge Printer Sample";

    public override int Priority => 0;

    public override Guid UniqueId { get; } = new("9D4B5E78-9DE7-40EA-86C2-E70078DDC8D1"); // Replace with your own unique ID

    public override bool SupportsDecoding => false;

    public override bool SupportsEncoding => false;

    public override bool Print(Guid credential, Guid badge, Guid cardholderGuid)
    {
        var credentialEntity = (Credential)Workspace.Sdk.GetEntity(credential);
        var cardholder = (Cardholder)Workspace.Sdk.GetEntity(cardholderGuid);

        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            var notificationService = Workspace.Services.Get<IBackgroundProcessNotificationService>();

            notificationService.Notify($"Starting print job for {cardholder.Name}'s badge with credential {credentialEntity.Name}...");

            IBadgeService service = Workspace.Services.Get<IBadgeService>();
            var badgeInformation = new BadgeInformation(badge, credential);

            service.BeginPrint(badgeInformation, printDialog, asyncResult =>
            {
                service.EndPrint(asyncResult);
                notificationService.Notify($"Print job for {cardholder.Name}'s badge with credential {credentialEntity.Name} completed successfully.");
            }, null);
        }
        else
        {
            // Notify the user that the print dialog was cancelled
            var notificationService = Workspace.Services.Get<IBackgroundProcessNotificationService>();
            notificationService.Notify($"Print job for {cardholder.Name}'s badge with credential {credentialEntity.Name} was cancelled.");
            return false;
        }

        return true;
    }
}