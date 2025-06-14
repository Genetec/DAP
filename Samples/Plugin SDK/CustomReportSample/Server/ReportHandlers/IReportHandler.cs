// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk.EventsArgs;

public interface IReportHandler
{
    Task HandleAsync(ReportQueryReceivedEventArgs args, CancellationToken token);
}