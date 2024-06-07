// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

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