// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Data;
using System.IO;
using Genetec.Sdk.ReportExport;

public class HtmlReportExporter : ReportExporter
{
    private readonly TextWriter m_writer;
    private bool m_tableStarted;

    public HtmlReportExporter(TextWriter writer, string reportTitle)
    {
        m_writer = writer;
        m_writer.WriteLine($"<html><head><title>{reportTitle}</title></head><body>");
        m_writer.WriteLine($"<h1>{reportTitle}</h1>");
    }

    public override QueryExportResult OnDataReady(QueryResultsBlock dataBlock)
    {
        try
        {
            if (!m_tableStarted)
            {
                m_writer.WriteLine("<table border='1'>");
                m_writer.WriteLine("<thead>");
                m_writer.WriteLine("<tr>");

                foreach (DataColumn column in dataBlock.Data.Columns)
                {
                    m_writer.WriteLine($"<th>{column.ColumnName}</th>");
                }

                m_writer.WriteLine("</tr>");
                m_writer.WriteLine("</thead>");
                m_writer.WriteLine("<tbody>");

                m_tableStarted = true;
            }

            foreach (DataRow row in dataBlock.Data.Rows)
            {
                m_writer.WriteLine("<tr>");
                foreach (var item in row.ItemArray)
                {
                    m_writer.WriteLine($"<td>{item}</td>");
                }
                m_writer.WriteLine("</tr>");
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
            if (m_tableStarted)
            {
                m_writer.WriteLine("</tbody>");
                m_writer.WriteLine("</table>");
            }

            m_writer.WriteLine("</body></html>");
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