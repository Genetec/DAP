namespace Genetec.Dap.CodeSamples
{
    using Genetec.Sdk.Workspace.Modules;

    public class SampleModule : Module
    {
        public override void Load()
        {
            Workspace.Sdk.LoggedOn += OnLoggedOn; 
        }

        private void OnLoggedOn(object sender, Sdk.LoggedOnEventArgs e)
        {
           
        }

        public override void Unload()
        {
           
        }
    }
}
