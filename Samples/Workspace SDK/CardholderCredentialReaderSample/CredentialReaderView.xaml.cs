// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

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
