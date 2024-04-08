namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Reactive.Linq;
    using System.Security.Cryptography.X509Certificates;
    using Sdk;
    using Sdk.Entities;
    using Sdk.EventsArgs;
    using Sdk.Plugin;
    using Sdk.Plugin.Interfaces;
    using Sdk.Plugin.Objects;

    public class SamplePlugin : Plugin, IPluginDatabaseSupport
    {
        private readonly PluginDatabase m_database = new PluginDatabase();

        private SecureKeyManager m_keyManager;

        private Role m_role;

        static SamplePlugin()
        {
            AssemblyResolver.Initialize();
        }

        public DatabaseManager DatabaseManager => m_database;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                m_database.Dispose();
        }

        protected override void OnPluginLoaded()
        {
            m_role = (Role)Engine.GetEntity(PluginGuid);
            m_keyManager = new SecureKeyManager(m_role.RestrictedConfiguration);
        }

        protected override void OnPluginStart()
        {

            m_database.ObserveState().Where(state => state == DatabaseState.Connected)
                .Subscribe(_ =>
                {

                    var data  = await m_database.GetCertificate();


                });

            //IObservable<string> onConfigurationChanged = Observable.Defer(() =>
            //{
            //    return Engine.OnEntityInvalidated().Where(args => args.EntityType == EntityType.Role && args.EntityGuid == m_role.Guid)
            //        .Select(_ => m_role.SpecificConfiguration)
            //        .StartWith(m_role.SpecificConfiguration)
            //        .DistinctUntilChanged();
            //});


            //onConfigurationChanged.Select(PluginConfiguration.Deserialize).CombineLatest(m_database.ObserveState(),
            //    (pluginConfiguration, databaseState) => (pluginConfiguration, databaseState))
            //    .Subscribe(tuple => ValidateCertificate(tuple.pluginConfiguration, tuple.databaseState));


            void ValidateCertificate(PluginConfiguration pluginConfiguration, DatabaseState databaseState)
            {
                const string context = "Certificate";

                if (pluginConfiguration.Certificate is null)
                {
                    ModifyPluginState(new PluginStateEntry(context, "No certificate has been set") { IsWarning = true, Details = ""});
                }
                else if (databaseState == DatabaseState.Connected)
                {
                    X509Certificate2 databaseCertificate = await m_database.GetCertificate();

                    if (databaseCertificate is null)
                    {
                        var key = m_keyManager.EncryptKey(pluginConfiguration.Certificate);

                        await m_database.SaveCertificate(databaseCertificate, key);
                    
                        ModifyPluginState(new PluginStateEntry(context, string.Empty));
                    }
                    else
                    {
                        ModifyPluginState(!CertificateComparer.Equals(databaseCertificate, pluginConfiguration.Certificate)
                            ? new PluginStateEntry(context, "certificates miss match") { IsWarning = true }
                            : new PluginStateEntry(context, string.Empty));
                    }

                  
                }
            }
        }

        protected override void OnQueryReceived(ReportQueryReceivedEventArgs args)
        {
        }
    }
}