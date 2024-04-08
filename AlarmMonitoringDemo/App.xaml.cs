namespace Genetec.Dap.CodeSamples
{
    using System.Windows;
    using EntityCacheDemo;

    public partial class App : Application
    {
        static App() => SdkResolver.Initialize();
    }
}