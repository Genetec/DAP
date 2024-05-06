// Copyright (C) 2024 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media.Imaging;
    using Properties;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Workspace.Modules;
    using Sdk.Workspace.Services;

    public class SampleModule : Module
    {
        public override void Load()
        {
            if (Workspace.ApplicationType == ApplicationType.ConfigTool)
            {
                Workspace.Sdk.LoggedOn += (sender, e) =>
                {
                    if (Workspace.Sdk.LoggedUser.IsAdministrator)
                    {
                        CreateDeviceCustomEntity();
                    }
                };

                var page = new CustomConfigPage();
                page.Initialize(Workspace);
                Workspace.Services.Get<IConfigurationService>().Register(page);
            }
        }

        private void CreateDeviceCustomEntity()
        {
            var config = (SystemConfiguration)Workspace.Sdk.GetEntity(SystemConfiguration.SystemConfigurationGuid);

            var capabilities = CustomEntityTypeCapabilities.CanBeFederated |
                               CustomEntityTypeCapabilities.IsVisible |
                               CustomEntityTypeCapabilities.MaintenanceMode |
                               CustomEntityTypeCapabilities.CreateDelete |
                               CustomEntityTypeCapabilities.MapSupport;

            var descriptor = new CustomEntityTypeDescriptor(AedUnitCustomEntityType.Id, Resources.CustomEntityTypeName,
                capabilities, new Version(1, 0))
            {
                ViewPrivilege = AedUnitCustomPrivilege.View,
                ModifyPrivilege = AedUnitCustomPrivilege.Modify,
                AddPrivilege = AedUnitCustomPrivilege.Add,
                DeletePrivilege = AedUnitCustomPrivilege.Delete,
                SmallIcon = new BitmapImage(new Uri("pack://application:,,,/ConfigPageSample;component/Resources/Images/SmallLogo.png")),
                LargeIcon = new BitmapImage(new Uri("pack://application:,,,/ConfigPageSample;component/Resources/Images/SmallLogo.png")),
                NameKey = nameof(Resources.CustomEntityTypeName),
                ResourceManagerTypeName = nameof(Resources),
                HierarchicalChildTypes = new List<EntityType> { EntityType.Camera },
                AuditableProperties = new List<CustomEntityAuditableProperty>
                {
                    new CustomEntityAuditableProperty
                    {
                        Hints = AuditPropertyHints.Xml,
                        Name = "Last inspection date",
                        NameKey = "LastInspectionDate",
                        Path = "/AedUnitInformation/LastInspectionDate",
                        SourceProperty = AuditPropertySource.Xml
                    },
                    new CustomEntityAuditableProperty
                    {
                        Hints = AuditPropertyHints.Xml,
                        Name = "Next scheduled maintenance",
                        NameKey = "NextScheduledMaintenance",
                        Path = "/AedUnitInformation/NextScheduledMaintenance",
                        SourceProperty = AuditPropertySource.Xml
                    },
                    new CustomEntityAuditableProperty
                    {
                        Hints = AuditPropertyHints.Xml,
                        Name = "Battery expiration date",
                        NameKey = "BatteryExpirationDate",
                        Path = "/AedUnitInformation/BatteryExpirationDate",
                        SourceProperty = AuditPropertySource.Xml
                    },
                    new CustomEntityAuditableProperty
                    {
                        Hints = AuditPropertyHints.Xml,
                        Name = "Pad expiration date",
                        NameKey = "PadExpirationDate",
                        Path = "/AedUnitInformation/PadExpirationDate",
                        SourceProperty = AuditPropertySource.Xml
                    }
                }
            };

            config.AddOrUpdateCustomEntityType(descriptor);
        }

        public override void Unload()
        {
        }
    }
}