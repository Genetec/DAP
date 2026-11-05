// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Sdk.ReportExport;

public class MarkdownReportExporter(TextWriter writer) : ReportExporter
{
    private bool m_headerWritten;

    public override QueryExportResult OnDataReady(QueryResultsBlock dataBlock)
    {
        try
        {
            if (!m_headerWritten)
            {
                writer.WriteLine($"| {string.Join(" | ", dataBlock.Data.Columns.Cast<DataColumn>().Select(c => EscapeCell(c.ColumnName)))} |");
                writer.WriteLine($"| {string.Join(" | ", dataBlock.Data.Columns.Cast<DataColumn>().Select(c => "---"))} |");
                m_headerWritten = true;
            }

            foreach (DataRow row in dataBlock.Data.Rows) writer.WriteLine($"| {string.Join(" | ", row.ItemArray.Select(item => EscapeCell(item.ToString())))} |");

            writer.Flush();
            return new QueryExportResult(true);
        }
        catch (Exception ex)
        {
            return new QueryExportResult(false, ex);
        }
    }

    private static string EscapeCell(string value) => WebUtility.HtmlEncode(Regex.Replace(value, @"[\\`*_{}\[\]()#+\-.!|~=]", @"\$0"))
        .Replace("\r\n", "<br>").Replace("\r", "<br>").Replace("\n", "<br>");

    public override void OnExportCompleted()
    {
        try
        {
            writer.Close();
        }
        catch
        {
            // Handle the exception if needed
        }
        finally
        {
            writer.Dispose();
        }
    }
}
