// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Data;
    using System.IO;
    using System.Linq;
    using Sdk.ReportExport;

    public class MarkdownReportExporter : ReportExporter
    {
        private readonly TextWriter m_writer;

        public MarkdownReportExporter(TextWriter writer)
        {
            m_writer = writer;
        }

        public override QueryExportResult OnDataReady(QueryResultsBlock dataBlock)
        {
            try
            {
                m_writer.WriteLine($"| {string.Join(" | ", dataBlock.Data.Columns.Cast<DataColumn>().Select(c => c.ColumnName))} |");
                m_writer.WriteLine($"| {string.Join(" | ", dataBlock.Data.Columns.Cast<DataColumn>().Select(c => "---"))} |");

                foreach (DataRow row in dataBlock.Data.Rows) m_writer.WriteLine($"| {string.Join(" | ", row.ItemArray.Select(item => item.ToString().Replace("|", "\\|")))} |");

                m_writer.Flush();
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
                m_writer.Close();
            }
            catch
            {
                // Handle the exception if needed
            }
            finally
            {
                m_writer.Dispose();
            }
        }
    }
}
