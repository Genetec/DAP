// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Sdk.Diagnostics.Logging.Core;

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
}