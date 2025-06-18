// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using Genetec.Sdk.Diagnostics.Logging;
    using Genetec.Sdk.Diagnostics.Logging.Core;

    static class StaticLogger
    {
        private static readonly Logger s_logger = Logger.CreateClassLogger(typeof(StaticLogger));

        public static void LogDebugMessage()
        {
            s_logger.TraceDebug("This is a debug message");
        }

        /// <summary>
        /// Executes a static debug method and returns a result message.
        /// </summary>
        /// <returns>A string containing the output message of this sample static debug method.</returns>
        [DebugMethod(Description = "Executes a sample static debug method", DisplayName = "Sample Static Debug Method", IsHidden = false, IsSecured = false)]
        [UserCommand("Genetec.Dap.Samples")]
        public static string ExecuteSampleDebugMethod()
        {
            return "This is the result of executing the sample static debug method.";
        }
    }
}
