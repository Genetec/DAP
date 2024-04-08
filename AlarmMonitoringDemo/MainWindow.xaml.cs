namespace Genetec.Dap.CodeSamples
{
    using System.Windows;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new AlarmMonitorViewModel();
        }
    }
}
