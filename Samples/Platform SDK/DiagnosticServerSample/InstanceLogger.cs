// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Genetec.Sdk.Diagnostics.Logging;
    using Genetec.Sdk.Diagnostics.Logging.Core;

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
        public string ExecuteSampleDebugMethod()
        {
            return "This is the result of executing the sample debug method.";
        }
    }
}