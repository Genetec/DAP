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