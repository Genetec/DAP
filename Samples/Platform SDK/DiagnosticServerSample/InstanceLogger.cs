// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using Genetec.Sdk.Diagnostics.Logging;
using Genetec.Sdk.Diagnostics.Logging.Core;

/// <summary>
/// This class demonstrates how to create an instance logger that logs debug messages and provides a sample debug method.
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

    /// <summary>
    /// Executes a debug method and returns a result message.
    /// </summary>
    /// <returns>A string containing the output message of this sample debug method.</returns>
    [DebugMethod(Description = "Executes a sample debug method", DisplayName = "Sample Debug Method", IsHidden = false, IsSecured = false)]
    [UserCommand("Genetec.Dap.Samples")]
    public string SampleDebugMethod()
    {
        return "This is the result of executing the sample debug method.";
    }

    /// <summary>
    /// Executes a static debug method and returns a result message.
    /// </summary>
    /// <returns>A string containing the output message of this sample static debug method.</returns>
    [DebugMethod(Description = "Executes a sample debug method", DisplayName = "Sample Debug Method With Parameter", IsHidden = false, IsSecured = false)]
    [UserCommand("Genetec.Dap.Samples")]
    public static string SampleDebugMethodWithParameter(string message)
    {
        return message;
    }
}