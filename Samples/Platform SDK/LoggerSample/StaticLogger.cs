// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using Sdk.Diagnostics.Logging.Core;

    static class StaticLogger
    {
        private static readonly Logger s_logger = Logger.CreateClassLogger(typeof(StaticLogger));

        public static void LogDebugMessage()
        {
            s_logger.TraceDebug("This is a debug message");
        }
    }
}
