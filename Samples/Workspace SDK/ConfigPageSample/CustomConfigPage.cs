// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Sdk;
using Sdk.Entities;
using Sdk.Workspace.Pages;

internal class CustomConfigPage : ConfigPage
{
    private DateTime m_batteryExpirationDate;

    private CustomEntity m_customEntity;

    private DateTime m_lastInspectionDate;

    private DateTime m_nextScheduledMaintenance;

    private DateTime m_padExpirationDate;

    // Specify for which entity type this configuration page is intended
    protected override EntityType EntityType => EntityType.CustomEntity;

    protected override Guid Entity
    {
        set
        {
            // Check if the entity is a custom entity of the expected type
            if (Workspace.Sdk.GetEntity(value) is CustomEntity entity && entity.CustomEntityType == AedUnitCustomEntityType.Id)
            {
                m_customEntity = entity; // Store the entity
                IsVisible = true; // Show the configuration page
            }
            else
            {
                IsVisible = false; // Hide the configuration page
            }
        }
    }

    // A public default constructor is required
    public CustomConfigPage()
    {
    }

    public DateTime LastInspectionDate
    {
        get => m_lastInspectionDate;
        set
        {
            if (SetProperty(ref m_lastInspectionDate, value))
            {
                IsDirty = true; // Mark the configuration as dirty, triggering the Save button to be enabled
            }
        }
    }

    public DateTime NextScheduledMaintenance
    {
        get => m_nextScheduledMaintenance;
        set
        {
            if (SetProperty(ref m_nextScheduledMaintenance, value))
            {
                IsDirty = true; // Mark the configuration as dirty, triggering the Save button to be enabled
            }
        }
    }

    public DateTime BatteryExpirationDate
    {
        get => m_batteryExpirationDate;
        set
        {
            if (SetProperty(ref m_batteryExpirationDate, value))
            {
                IsDirty = true; // Mark the configuration as dirty, triggering the Save button to be enabled
            }
        }
    }

    public DateTime PadExpirationDate
    {
        get => m_padExpirationDate;
        set
        {
            if (SetProperty(ref m_padExpirationDate, value))
            {
                IsDirty = true; // Mark the configuration as dirty, triggering the Save button to be enabled
            }
        }
    }

    protected override string Name => base.Name; // Optional: The name of the configuration page. Default is "Properties".

    protected override ImageSource Image => base.Image; // Optional: The image of the configuration page. Default is the "Properties" image.

    private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(storage, value))
        {
            return false;
        }

        storage = value;
        OnPropertyChanged(propertyName);

        return true;
    }

    protected override void Initialize()
    {
        View = new CustomConfigPageView { DataContext = this }; // Assign the view to the config page and associate it with this view model
    }

    // Load the configuration from the custom entity
    protected override void Refresh()
    {
        AedUnitInformation configuration = AedUnitInformation.Deserialize(m_customEntity.Xml);
        LastInspectionDate = configuration.LastInspectionDate;
        NextScheduledMaintenance = configuration.NextScheduledMaintenance;
        BatteryExpirationDate = configuration.BatteryExpirationDate;
        PadExpirationDate = configuration.PadExpirationDate;
        IsDirty = false;
    }

    // Validate the configuration before saving
    protected override void BeforeSave(bool beforeUnloading, out bool cancelSaveOperation)
    {
        if (NextScheduledMaintenance <= LastInspectionDate)
        {
            cancelSaveOperation = true;
        }

        cancelSaveOperation = false;
    }

    // Save the configuration to the custom entity
    protected override void Save()
    {
        m_customEntity.Xml = new AedUnitInformation { LastInspectionDate = LastInspectionDate, NextScheduledMaintenance = NextScheduledMaintenance, BatteryExpirationDate = BatteryExpirationDate, PadExpirationDate = PadExpirationDate }.Serialize();
    }
}