namespace Genetec.Dap.CodeSamples
{
    using System.Windows.Controls;

    public partial class SampleOptionsView : UserControl
    {
        public SampleOptionsView(SampleOptionPage page)
        {
            InitializeComponent();
            DataContext = page;
        }
    }
}
