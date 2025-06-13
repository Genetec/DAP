// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Data;
    using System.IO;
    using Sdk.ReportExport;
    using System.Xml;

    public class XmlReportExporter : ReportExporter
    {
        private readonly DataTable m_dataTable;
        private readonly XmlWriter m_xmlWriter;

        public XmlReportExporter(TextWriter textWriter)
        {
            m_xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings { Indent = true });
            m_dataTable = new DataTable("Results");
        }

        public override QueryExportResult OnDataReady(QueryResultsBlock dataBlock)
        {
            try
            {
                if (m_dataTable.Columns.Count == 0)
                {
                    foreach (DataColumn column in dataBlock.Data.Columns)
                    {
                        m_dataTable.Columns.Add(column.ColumnName, column.DataType);
                    }
                }

                foreach (DataRow row in dataBlock.Data.Rows)
                {
                    m_dataTable.ImportRow(row);
                }

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
                m_dataTable.WriteXml(m_xmlWriter);
                m_xmlWriter.Flush();
            }
            catch
            {
                // Handle the exception if needed
            }
            finally
            {
                m_xmlWriter.Dispose();
            }
        }
    }
}