// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Data;
using System.IO;
using System.Linq;
using Sdk.ReportExport;

public class MarkdownReportExporter(TextWriter writer) : ReportExporter
{
    public override QueryExportResult OnDataReady(QueryResultsBlock dataBlock)
    {
        try
        {
            writer.WriteLine($"| {string.Join(" | ", dataBlock.Data.Columns.Cast<DataColumn>().Select(c => c.ColumnName))} |");
            writer.WriteLine($"| {string.Join(" | ", dataBlock.Data.Columns.Cast<DataColumn>().Select(c => "---"))} |");

            foreach (DataRow row in dataBlock.Data.Rows) writer.WriteLine($"| {string.Join(" | ", row.ItemArray.Select(item => item.ToString().Replace("|", "\\|")))} |");

            writer.Flush();
            return new QueryExportResult(true);
        }
        catch (Exception ex)
        {
            return new QueryExportResult(false, ex);
        }
    }

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