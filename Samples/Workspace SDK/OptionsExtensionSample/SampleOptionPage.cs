// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
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
}