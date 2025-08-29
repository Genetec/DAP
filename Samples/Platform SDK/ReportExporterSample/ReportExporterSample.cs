// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

public class ReportExporterSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        var query = (CardholderActivityQuery)engine.ReportManager.CreateReportQuery(ReportType.CardholderActivity);
        query.MaximumResultCount = 10000;

        using var xmlWriter = new StreamWriter("CardholderActivity.xml", false, Encoding.UTF8);
        using var jsonWriter = new StreamWriter("CardholderActivity.json", false, Encoding.UTF8);
        using var csvWriter = new StreamWriter("CardholderActivity.csv", false, Encoding.UTF8);
        using var markDownWriter = new StreamWriter("CardholderActivity.md", false, Encoding.UTF8);
        using var htmlWriter = new StreamWriter("CardholderActivity.html", false, Encoding.UTF8);

        query.Exporter = new AggregateReportExporter(
            new XmlReportExporter(xmlWriter),
            new JsonReportExporter(jsonWriter),
            new CsvReportExporter(csvWriter),
            new MarkdownReportExporter(markDownWriter),
            new HtmlReportExporter(htmlWriter, "Cardholder Activity"));

        await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    }
}