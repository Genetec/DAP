// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

public static class CsvFormatting
{
    public static string Escape(object value)
    {
        string text = value?.ToString() ?? string.Empty;
        if (value is string)
        {
            string trimmed = text.TrimStart();
            // Quoting CSV does not stop a spreadsheet from evaluating text as a formula.
            if ((trimmed.Length > 0 && "=+-@".IndexOf(trimmed[0]) >= 0) ||
                (text.Length > 0 && (char.IsControl(text[0]) || text[0] == '\uFEFF')))
                text = "'" + text;
        }

        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }
}
