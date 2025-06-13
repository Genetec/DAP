// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System.Windows.Controls;

    public partial class BackgroundProcessPageView : UserControl
    {
        public BackgroundProcessPageView()
        {
            InitializeComponent();
        }

        public BackgroundProcessPageView(object dataContext) : this()
        {
            DataContext = dataContext;
        }
    }
}
