namespace Genetec.Dap.CodeSamples
{
    using Genetec.Sdk.Plugin;

    public class SampleDatabaseManager : DatabaseManager
    {

        public DatabaseConfiguration Configuration { get; private set; }

        public override string GetSpecificCreationScript(string databaseName)
        {
            
        }

        public override void SetDatabaseInformation(DatabaseConfiguration databaseConfiguration)
        {
            Configuration = Configuration;
        }
    }
}