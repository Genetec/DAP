// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples;

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