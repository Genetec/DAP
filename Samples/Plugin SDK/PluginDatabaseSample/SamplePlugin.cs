// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Genetec.Sdk.Diagnostics.Logging.Core;
    using Genetec.Sdk.EventsArgs;
    using Genetec.Sdk.Plugin;
    using Genetec.Sdk.Plugin.Interfaces;
    using Genetec.Sdk.Plugin.Objects;

    [PluginProperty(typeof(SamplePluginDescriptor))]
    public class SamplePlugin : Plugin, IPluginDatabaseSupport // Implement the IPluginDatabaseSupport interface to support database operations
    {
        private readonly SampleDatabaseManager m_databaseManager = new();

        public DatabaseManager DatabaseManager => m_databaseManager;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_databaseManager.Dispose();
            }
        }

        protected override void OnPluginLoaded()
        {
            // Subscribe to the DatabaseStateChanged event to know when the database is connected
            m_databaseManager.DatabaseStateChanged += (sender, e) =>
            {
                Logger.TraceDebug($"Database state Changed to {m_databaseManager.State}");

                if (m_databaseManager.State == DatabaseState.Connected) // Insert a log message when the database is connected
                { 
                    m_databaseManager.InsertLog(DateTime.UtcNow, 1, "This is a test log message.");
                }
            };
        }

        protected override void OnPluginStart()
        {
            ModifyPluginState(new PluginStateEntry("PluginState", "Plugin started"));
        }

        protected override void OnQueryReceived(ReportQueryReceivedEventArgs args)
        {
        }
    }
}