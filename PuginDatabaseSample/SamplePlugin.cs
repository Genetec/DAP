namespace Genetec.Dap.CodeSamples
{
    using Sdk.EventsArgs;
    using Sdk.Plugin;
    using Sdk.Plugin.Interfaces;

    public class SamplePlugin : Plugin, IPluginDatabaseSupport
    {
        public DatabaseManager DatabaseManager { get; } = new SampleDatabaseManager();

        protected override void Dispose(bool disposing)
        {
        }

        protected override void OnPluginLoaded()
        {
        }

        protected override void OnPluginStart()
        {
        }

        protected override void OnQueryReceived(ReportQueryReceivedEventArgs args)
        {
        }
    }
}