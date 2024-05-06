// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Sdk.Workspace.Pages;

    public partial class CustomReportFilter : ReportFilter, INotifyPropertyChanged
    {
        private string m_message;

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
                }
            }
        }

        protected override string FilterName => "Custom filter";

        protected override bool IsStateValid => !string.IsNullOrEmpty(Message);

        protected override string FilterData
        {
            get
            {
                var data = new CustomReportFilterData
                {
                    Message = Message
                };
                return data.Serialize();
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var data = CustomReportFilterData.Deserialize(value);
                    Message = data.Message;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

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