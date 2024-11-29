// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Linq;
using Genetec.Sdk.ReportExport;

class AggregateReportExporter(params ReportExporter[] exporters) : ReportExporter
{
    private readonly List<ReportExporter> m_exporters = exporters.ToList();

    public override QueryExportResult OnDataReady(QueryResultsBlock dataBlock)
    {
        List<Exception> exceptions = m_exporters.Select(exporter => exporter.OnDataReady(dataBlock)).Where(result => !result.Success).Select(result => result.Exception).ToList();
        return exceptions.Any() ? new QueryExportResult(false, new AggregateException(exceptions)) : new QueryExportResult(true);
    }

    public override void OnExportCompleted()
    {
        foreach (ReportExporter exporter in m_exporters)
        {
            exporter.OnExportCompleted();
        }
    }
}