// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using Sdk.ReportExport;

    public class CsvReportExporter : ReportExporter
    {
        private bool m_headerWritten;
        private readonly TextWriter m_writer;

        public CsvReportExporter(TextWriter writer)
        {
            m_writer = writer;
        }

        public override QueryExportResult OnDataReady(QueryResultsBlock dataBlock)
        {
            try
            {
                if (!m_headerWritten)
                {
                    IEnumerable<string> columnNames = dataBlock.Data.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
                    m_writer.WriteLine(string.Join(",", columnNames));
                    m_headerWritten = true;
                }

                foreach (DataRow row in dataBlock.Data.Rows)
                {
                    IEnumerable<string> fields = row.ItemArray.Select(field => $"\"{field.ToString().Replace("\"", "\"\"")}\"");
                    m_writer.WriteLine(string.Join(",", fields));
                }

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
