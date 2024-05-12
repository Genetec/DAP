// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Genetec.Sdk.ReportExport;

    public class AggregateReportExporter : ReportExporter
    {
        private readonly List<ReportExporter> m_exporters;

        public AggregateReportExporter(params ReportExporter[] exporters)
        {
            m_exporters = exporters.ToList();
        }

        public override QueryExportResult OnDataReady(QueryResultsBlock dataBlock)
        {
            var exceptions = m_exporters.Select(exporter => exporter.OnDataReady(dataBlock)).Where(result => !result.Success).Select(result => result.Exception).ToList();
            return exceptions.Any() ? new QueryExportResult(false, new AggregateException(exceptions)) : new QueryExportResult(true);
        }

        public override void OnExportCompleted()
        {
            foreach (var exporter in m_exporters)
            {
                exporter.OnExportCompleted();
            }
        }
    }

}
