// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Client;

using Prism.Mvvm;

// An editable name/value row shared by the query-parameter and header grids.
public class EditableNameValue : BindableBase
{
    private string m_name;
    private string m_value;

    public string Name
    {
        get => m_name;
        set => SetProperty(ref m_name, value);
    }

    public string Value
    {
        get => m_value;
        set => SetProperty(ref m_value, value);
    }

    public NameValuePair ToPair() => new() { Name = Name, Value = Value };
}
