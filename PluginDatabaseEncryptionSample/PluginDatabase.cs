namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Sdk.Diagnostics.Logging.Core;
    using Sdk.Plugin;
    using Sdk.Plugin.Objects;

    public class PluginDatabase : DatabaseManager, IDisposable
    {
        public DatabaseConfiguration Configuration { get; private set; }

        private readonly Logger m_logger;

        public PluginDatabase()
        {
            m_logger = Logger.CreateInstanceLogger(this);
        }

        public DatabaseState State { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public event EventHandler<EventArgs> DatabaseStateChanged;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_logger.TraceDebug("Disposing");
                m_logger.Dispose();
            }
        }



        public override string GetSpecificCreationScript(string databaseName)
        {
            return null;
        }

        public override void OnDatabaseStateChanged(DatabaseNotification notification)
        {
            m_logger.TraceInformation($"Database state changed from {State} to {notification.State}");
            State = notification.State;
            DatabaseStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public override void SetDatabaseInformation(DatabaseConfiguration databaseConfiguration)
        {
            Configuration = databaseConfiguration;
        }

        public async Task<byte[]> GetCertificate()
        {
            using (SqlConnection connection = Configuration.CreateSqlConnection())
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("SELECT CertificateData FROM Certificates", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return reader.GetFieldValue<byte[]>(0);
                        }
                    }
                }
            }

            return null;
        }

        public async Task SaveCertificate(byte[] certificate)
        {
            using (SqlConnection connection = Configuration.CreateSqlConnection())
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("INSERT INTO Certificates (CertificateData) VALUES (@CertificateData)", connection))
                {
                    command.Parameters.AddWithValue("@CertificateData", certificate);
                    await command.ExecuteNonQueryAsync();
                }
            }

        }
    }
}