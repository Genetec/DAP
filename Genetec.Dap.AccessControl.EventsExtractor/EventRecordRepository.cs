namespace Genetec.Dap.AccessControl
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Sdk.Diagnostics.Logging.Core;

    public class EventRecordRepository : IEventRecordRepository, IDisposable
    {
        private readonly string m_connectionString;

        private readonly Logger m_logger;

        public EventRecordRepository(string connectionString)
        {
            m_logger = Logger.CreateInstanceLogger(this);
            m_connectionString = connectionString;
        }

        public void Dispose()
        {
            m_logger.Dispose();
        }

        public async Task InsertRecords(IEnumerable<EventRecord> records)
        {
            var dataTable = ConvertToDataTable();
            using var connection = new SqlConnection(m_connectionString);
            await connection.OpenAsync();
            using (var bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.DestinationTableName = "genetec_event_data";
                MapColumns(bulkCopy);

                m_logger.TraceInformation($"Inserting {dataTable.Rows.Count} records");
                await bulkCopy.WriteToServerAsync(dataTable);
            }

            DataTable ConvertToDataTable()
            {
                var table = new DataTable();
                table.Columns.Add("EMPLID", typeof(string));
                table.Columns.Add("FullName", typeof(string));
                table.Columns.Add("BadgeId", typeof(string));
                table.Columns.Add("EventID", typeof(string));
                table.Columns.Add("ReaderName", typeof(string));
                table.Columns.Add("DoorName", typeof(string));
                table.Columns.Add("ProfileName", typeof(string));
                table.Columns.Add("EventDateLocalTime", typeof(DateTime));
                table.Columns.Add("EventDateGMT", typeof(DateTime));
                table.Columns.Add("EventType", typeof(string));
                table.Columns.Add("CreatedDate", typeof(DateTime));
                table.Columns.Add("SiteName", typeof(string));
                table.Columns.Add("AccessManager", typeof(string));

                foreach (var record in records)
                {
                    var row = table.NewRow();
                    row["EMPLID"] = record.EMPLID ?? (object)DBNull.Value;
                    row["FullName"] = record.FullName ?? (object)DBNull.Value;
                    row["BadgeId"] = record.BadgeId;
                    row["EventID"] = record.EventID ?? (object)DBNull.Value;
                    row["ReaderName"] = record.ReaderName ?? (object)DBNull.Value;
                    row["DoorName"] = record.DoorName ?? (object)DBNull.Value;
                    row["ProfileName"] = record.ProfileName ?? (object)DBNull.Value;
                    row["EventDateLocalTime"] = record.EventDateLocalTime ?? (object)DBNull.Value;
                    row["EventDateGMT"] = record.EventDateGMT ?? (object)DBNull.Value;
                    row["EventType"] = record.EventType ?? (object)DBNull.Value;
                    row["CreatedDate"] = record.CreatedDate ?? (object)DBNull.Value;
                    row["SiteName"] = record.SiteName ?? (object)DBNull.Value;
                    row["AccessManager"] = record.AccessManager ?? (object)DBNull.Value;
                    table.Rows.Add(row);
                }

                return table;
            }

            void MapColumns(SqlBulkCopy bulkCopy)
            {
                bulkCopy.ColumnMappings.Add(nameof(EventRecord.EMPLID), "EMPLID");
                bulkCopy.ColumnMappings.Add(nameof(EventRecord.FullName), "FullName");
                bulkCopy.ColumnMappings.Add(nameof(EventRecord.BadgeId), "BadgeId");
                bulkCopy.ColumnMappings.Add(nameof(EventRecord.EventID), "EventID");
                bulkCopy.ColumnMappings.Add(nameof(EventRecord.ReaderName), "ReaderName");
                bulkCopy.ColumnMappings.Add(nameof(EventRecord.DoorName), "DoorName");
                bulkCopy.ColumnMappings.Add(nameof(EventRecord.ProfileName), "ProfileName");
                bulkCopy.ColumnMappings.Add(nameof(EventRecord.EventDateLocalTime), "EventDateLocalTime");
                bulkCopy.ColumnMappings.Add(nameof(EventRecord.EventDateGMT), "EventDateGMT");
                bulkCopy.ColumnMappings.Add(nameof(EventRecord.EventType), "EventType");
                bulkCopy.ColumnMappings.Add(nameof(EventRecord.CreatedDate), "CreatedDate");
                bulkCopy.ColumnMappings.Add(nameof(EventRecord.SiteName), "SiteName");
                bulkCopy.ColumnMappings.Add(nameof(EventRecord.AccessManager), "AccessManager");
            }
        }
    }
}