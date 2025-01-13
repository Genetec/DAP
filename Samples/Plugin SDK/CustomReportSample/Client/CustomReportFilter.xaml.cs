// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Client;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Sdk.Entities;
using Sdk.Entities.CustomEvents;
using Sdk.Workspace.Pages;

public partial class CustomReportFilter : ReportFilter, INotifyPropertyChanged
{
    private CustomEvent m_customEvent;
    private decimal m_decimalValue;
    private bool m_enabled;
    private string m_message;
    private int m_numericValue;

    public CustomReportFilter()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string Message
    {
        get => m_message;
        set
        {
            if (SetProperty(ref m_message, value))
            {
                OnModified();

                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Message cannot be empty.");
                }
            }
        }
    }

    public CustomEvent CustomEvent
    {
        get => m_customEvent;
        set
        {
            if (SetProperty(ref m_customEvent, value))
            {
                OnModified();
            }
        }
    }

    public bool Enabled
    {
        get => m_enabled;
        set
        {
            if (SetProperty(ref m_enabled, value))
            {
                OnModified();
            }
        }
    }

    public decimal Decimal
    {
        get => m_decimalValue;
        set
        {
            if (SetProperty(ref m_decimalValue, value))
            {
                OnModified();
            }
        }
    }

    public int NumericValue
    {
        get => m_numericValue;
        set
        {
            if (SetProperty(ref m_numericValue, value))
            {
                OnModified();
            }
        }
    }

    protected override string FilterName => "Custom filter";

    protected override bool IsStateValid => !string.IsNullOrEmpty(Message);

    protected override string FilterData
    {
        get
        {
            return new CustomReportFilterData
            {
                Message = Message,
                CustomEvent = CustomEvent?.Id,
                Enabled = Enabled,
                DecimalValue = Decimal,
                NumericValue = NumericValue
            }.Serialize();
        }
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                CustomReportFilterData data = CustomReportFilterData.Deserialize(value);
                Message = data.Message;
                CustomEvent = CustomEvents.FirstOrDefault(customEvent => customEvent.Id == data.CustomEvent);
                Enabled = data.Enabled;
                Decimal = data.DecimalValue;
                NumericValue = data.NumericValue;
            }
        }
    }

    public ObservableCollection<CustomEvent> CustomEvents { get; } = [];

    public event PropertyChangedEventHandler PropertyChanged;

    protected override void Initialize()
    {
        var systemConfiguration = (SystemConfiguration)Workspace.Sdk.GetEntity(SystemConfiguration.SystemConfigurationGuid);
        foreach (CustomEvent customEvent in systemConfiguration.CustomEventService.CustomEvents)
        {
            CustomEvents.Add(customEvent);
        }
    }

    private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return false;
        }

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}