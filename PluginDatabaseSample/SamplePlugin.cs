// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using Sdk.Diagnostics.Logging.Core;
    using Sdk.EventsArgs;
    using Sdk.Plugin;
    using Sdk.Plugin.Interfaces;
    using Sdk.Plugin.Objects;

    [PluginProperty(typeof(SamplePluginDescriptor))]
    public class SamplePlugin : Plugin, IPluginDatabaseSupport
    {
        private readonly SampleDatabaseManager m_databaseManager = new SampleDatabaseManager();

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
            m_databaseManager.DatabaseStateChanged += (sender, e) =>
            {
                Logger.TraceDebug($"Database state Changed to {m_databaseManager.State}");

                if (m_databaseManager.State == DatabaseState.Connected)
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