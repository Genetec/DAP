﻿// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System.Windows.Controls;

    public partial class CredentialReaderView : UserControl
    {
        public CredentialReaderView(SampleCardholderCredentialReader credentialReader)
        {
            DataContext = credentialReader;
            InitializeComponent();
        }
    }
}
