namespace Genetec.Dap.CodeSamples
{
    using System.Windows.Controls;

    public partial class CredentialReaderView : UserControl
    {
        public CredentialReaderView(BatchCredentialReader credentialReader)
        {
            DataContext = credentialReader;
            InitializeComponent();
        }
    }
}
