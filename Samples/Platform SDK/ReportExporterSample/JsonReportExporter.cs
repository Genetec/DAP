// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.IO;
using Newtonsoft.Json;
using Sdk.ReportExport;

public class JsonReportExporter : ReportExporter
{
    private readonly TextWriter m_writer;

    public JsonReportExporter(TextWriter writer)
    {
        m_writer = writer;
        m_writer.Write("[");
    }

    public override QueryExportResult OnDataReady(QueryResultsBlock dataBlock)
    {
        try
        {
            string json = JsonConvert.SerializeObject(dataBlock.Data, Formatting.Indented);
            m_writer.Write($"{json.TrimEnd(']', '\r', '\n')},");
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
            m_writer.Write("]");
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