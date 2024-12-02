// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Data;
using System.IO;
using Sdk.ReportExport;
using System.Xml;

class XmlReportExporter(TextWriter textWriter) : ReportExporter
{
    private readonly DataTable m_dataTable = new("Results");
    private readonly XmlWriter m_xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings { Indent = true });

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