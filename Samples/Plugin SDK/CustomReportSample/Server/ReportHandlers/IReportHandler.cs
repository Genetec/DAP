// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.EventsArgs;

public interface IReportHandler
{
    Task<ReportError> HandleAsync(ReportQueryReceivedEventArgs args, CancellationToken token);
}