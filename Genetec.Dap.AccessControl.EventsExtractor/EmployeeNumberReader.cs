namespace Genetec.Dap.AccessControl
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Sdk.Diagnostics.Logging.Core;

    public class EmployeeNumberReader : IEmployeeNumberReader, IDisposable
    {
        private readonly string m_connectionString;

        private readonly Logger m_logger;

        public EmployeeNumberReader(string connectionString)
        {
            m_logger = Logger.CreateInstanceLogger(this);
            m_connectionString = connectionString;
        }

        public void Dispose()
        {
            m_logger.Dispose();
        }

        public async Task<HashSet<string>> GetAllEmployeeNumbers()
        {
            m_logger.TraceInformation("Reading employee numbers...");

            var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (var connection = new SqlConnection(m_connectionString))
            {
                const string query = "SELECT EmpNo FROM [Emp&Card_No]";
                using (var command = new SqlCommand(query, connection))
                {
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            if (reader["EmpNo"] is string empNo)
                                results.Add(empNo.Trim());
                    }
                }
            }

            m_logger.TraceInformation($"{results.Count} employee numbers found");
            return results;
        }
    }
}