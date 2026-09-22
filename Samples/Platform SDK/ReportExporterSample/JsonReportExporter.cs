// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sdk.ReportExport;

public class JsonReportExporter : ReportExporter
{
    private readonly JsonTextWriter m_writer;

    public JsonReportExporter(TextWriter writer)
    {
        m_writer = new JsonTextWriter(writer) { Formatting = Formatting.Indented, CloseOutput = true };
        m_writer.WriteStartArray();
    }

    public override QueryExportResult OnDataReady(QueryResultsBlock dataBlock)
    {
        try
        {
            foreach (JToken row in JArray.FromObject(dataBlock.Data))
            {
                row.WriteTo(m_writer);
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
            m_writer.WriteEndArray();
            m_writer.Flush();
        }
        catch
        {
            // Handle the exception if needed
        }
        finally
        {
            m_writer.Close();
        }
    }
}
