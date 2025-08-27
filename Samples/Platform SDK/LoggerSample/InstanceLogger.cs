// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Sdk.Diagnostics.Logging.Core;

/// <summary>
/// This class demonstrates how to create an instance logger that logs debug messages.
/// </summary>
class InstanceLogger : IDisposable
{
    private readonly Logger m_logger;

    public InstanceLogger()
    {
        m_logger = Logger.CreateInstanceLogger(this);
    }

    public void Dispose()
    {
        m_logger.Dispose();
    }

    public void LogDebugMessage()
    {
        m_logger.TraceDebug("This is a debug message");
    }
}