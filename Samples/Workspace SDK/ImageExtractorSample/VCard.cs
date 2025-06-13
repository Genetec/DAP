// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System.Collections.Generic;
    using System.Windows.Media;

    public class VCard
    {
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public List<string> Emails { get; } = new List<string>();
        
        public string Note { get; set; }
        
        public ImageSource Picture { get; set; }
    }
}