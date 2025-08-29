// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Sdk.ReportExport;

public class CsvReportExporter(TextWriter writer) : ReportExporter
{
    private bool m_headerWritten;

    public override QueryExportResult OnDataReady(QueryResultsBlock dataBlock)
    {
        try
        {
            if (!m_headerWritten)
            {
                IEnumerable<string> columnNames = dataBlock.Data.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
                writer.WriteLine(string.Join(",", columnNames));
                m_headerWritten = true;
            }

            foreach (DataRow row in dataBlock.Data.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => $"\"{field.ToString().Replace("\"", "\"\"")}\"");
                writer.WriteLine(string.Join(",", fields));
            }

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