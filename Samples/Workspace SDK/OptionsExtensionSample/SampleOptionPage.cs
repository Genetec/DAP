// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Sdk.Workspace.Options;

public class SampleOptionPage : OptionPage, INotifyPropertyChanged
{
    private readonly SampleOptionsExtensions m_extension;

    private Color m_color;

    private DateTime m_dateTime;

    private int m_number;

    private string m_text;

    public SampleOptionPage(SampleOptionsExtensions extension)
    {
        m_extension = extension;
        View = new SampleOptionsView(this);
    }

    public int Number
    {
        get => m_number;
        set
        {
            if (SetProperty(ref m_number, value))
            {
                OnModified();
            }
        }
    }

    public DateTime DateTime
    {
        get => m_dateTime;
        set
        {
            if (SetProperty(ref m_dateTime, value))
            {
                OnModified();
            }
        }
    }

    public Color Color
    {
        get => m_color;
        set
        {
            if (SetProperty(ref m_color, value))
            {
                OnModified();
            }
        }
    }

    public string Text
    {
        get => m_text;
        set
        {
            if (SetProperty(ref m_text, value))
            {
                OnModified();
            }
        }
    }

    public override UIElement View { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    public override void Load()
    {
        Text = m_extension.Text;
        Number = m_extension.Number;
        DateTime = m_extension.DateTime;
        Color = m_extension.Color;
    }

    public override void Save()
    {
        m_extension.Text = Text;
        m_extension.Number = Number;
        m_extension.DateTime = DateTime;
        m_extension.Color = Color;
    }

    public override bool Validate()
    {
        return true;
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

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}