// Copyright (C) 2024 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Runtime.CompilerServices;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Workspace.Pages;

    internal class CustomConfigPage : ConfigPage
    {
        private CustomEntity m_customEntity;

        private DateTime m_padExpirationDate;

        private DateTime m_batteryExpirationDate;

        private DateTime m_nextScheduledMaintenance;

        private DateTime m_lastInspectionDate;

        protected override EntityType EntityType => EntityType.CustomEntity;

        protected override Guid Entity
        {
            set
            {
                if (Workspace.Sdk.GetEntity(value) is CustomEntity entity && entity.CustomEntityType == new Guid("8385D04C-F04A-4125-81E9-D1C66AFDE572"))
                {
                    m_customEntity = entity;
                    IsVisible = true;
                }
                else
                {
                    IsVisible = false;
                }
            }
        }

        public DateTime LastInspectionDate
        {
            get => m_lastInspectionDate;
            set
            {
                if (SetProperty(ref m_lastInspectionDate, value))
                {
                    IsDirty = true;
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
                    IsDirty = true;
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
                    IsDirty = true;
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
                    IsDirty = true;
                }
            }
        }

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
            View = new CustomConfigPageView { DataContext = this };
        }

        protected override void Refresh()
        {
            AedUnitInformation configuration = AedUnitInformation.Deserialize(m_customEntity.Xml);
            LastInspectionDate = configuration.LastInspectionDate;
            NextScheduledMaintenance = configuration.NextScheduledMaintenance;
            BatteryExpirationDate = configuration.BatteryExpirationDate;
            PadExpirationDate = configuration.PadExpirationDate;
            IsDirty = false;
        }

        protected override void Save()
        {
            m_customEntity.Xml = new AedUnitInformation
            {
                LastInspectionDate = LastInspectionDate,
                NextScheduledMaintenance = NextScheduledMaintenance,
                BatteryExpirationDate = BatteryExpirationDate,
                PadExpirationDate = PadExpirationDate
            }.Serialize();
        }
    }
}