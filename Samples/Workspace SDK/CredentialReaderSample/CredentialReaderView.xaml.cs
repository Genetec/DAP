namespace Genetec.Dap.CodeSamples
// Licensed under the Apache License, Version 2.0

    using System.Windows.Controls;

    public partial class CredentialReaderView : UserControl
    {
        public CredentialReaderView(SampleCredentialReader credentialReader)
        {
            DataContext = credentialReader;
            InitializeComponent();
        }
    }
}
