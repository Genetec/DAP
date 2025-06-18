namespace Genetec.Dap.CodeSamples
// Licensed under the Apache License, Version 2.0

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
