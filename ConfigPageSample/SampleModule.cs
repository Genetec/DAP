// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media.Imaging;
    using Genetec.Sdk.Privileges;
    using Properties;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Workspace.Modules;
    using Sdk.Workspace.Services;

    public class SampleModule : Module
    {
        public override void Load()
        {
            if (Workspace.ApplicationType is ApplicationType.ConfigTool)
            {
                Workspace.Sdk.LoggedOn += (sender, e) =>
                {
                    if (Workspace.Sdk.LoggedUser.IsAdministrator)
                    {
                        CreateCustomPrivileges();
                        CreateDeviceCustomEntity();
                    }
                };

                // Initialize and register the custom configuration page
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
                    new()
                    {
                        Hints = AuditPropertyHints.Xml,
                        Name = "Last inspection date",
                        NameKey = "LastInspectionDate",
                        Path = "AedUnitInformation/LastInspectionDate",
                        SourceProperty = AuditPropertySource.Xml
                    },
                    new()
                    {
                        Hints = AuditPropertyHints.Xml,
                        Name = "Next scheduled maintenance",
                        NameKey = "NextScheduledMaintenance",
                        Path = "AedUnitInformation/NextScheduledMaintenance",
                        SourceProperty = AuditPropertySource.Xml
                    },
                    new()
                    {
                        Hints = AuditPropertyHints.Xml,
                        Name = "Battery expiration date",
                        NameKey = "BatteryExpirationDate",
                        Path = "AedUnitInformation/BatteryExpirationDate",
                        SourceProperty = AuditPropertySource.Xml
                    },
                    new()
                    {
                        Hints = AuditPropertyHints.Xml,
                        Name = "Pad expiration date",
                        NameKey = "PadExpirationDate",
                        Path = "AedUnitInformation/PadExpirationDate",
                        SourceProperty = AuditPropertySource.Xml
                    }
                }
            };

            config.AddOrUpdateCustomEntityType(descriptor);
        }

        private void CreateCustomPrivileges()
        {
            Guid privilegeGroupId = Guid.Parse("864E018E-8B50-41B7-80F5-4F1BB2BBED6E");

            Workspace.Sdk.SecurityManager.RegisterPrivileges(new List<PrivilegeRegistration>
            {
                new(id: privilegeGroupId,
                    type: PrivilegeType.Group,
                    description: "AED Unit",
                    details: "AED Unit",
                    priority: 2,
                    icon: null,
                    parentId: Guid.Parse("38ACC600-4A32-44ff-8DF5-0797D888930B")),
                new(id: AedUnitCustomPrivilege.View,
                    type: PrivilegeType.Entity,
                    description: "View AED unit properties",
                    details: "Allows the user to view the configuration of AED units.",
                    priority: 2,
                    icon: null,
                    parentId: privilegeGroupId),
                new(id: AedUnitCustomPrivilege.Modify,
                    type: PrivilegeType.Entity,
                    description: "Modify AED unit properties",
                    details: "Allows the user to modify the configuration of AED units.",
                    priority: 2,
                    icon: null,
                    parentId:  AedUnitCustomPrivilege.View),
                new(id: AedUnitCustomPrivilege.Add,
                    type: PrivilegeType.Entity,
                    description: "Add AED units",
                    details: "Allows the user to add AED units.",
                    priority: 2,
                    icon: null,
                    parentId: AedUnitCustomPrivilege.Modify),
                new(id: AedUnitCustomPrivilege.Delete,
                    type: PrivilegeType.Entity,
                    description: "Remove AED units",
                    details: "Allows the user to remove AED units.",
                    priority: 2,
                    icon: null,
                    parentId: AedUnitCustomPrivilege.Modify
                )
            });
        }

        public override void Unload()
        {
        }
    }
}