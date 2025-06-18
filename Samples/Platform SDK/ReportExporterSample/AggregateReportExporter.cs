// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

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
