namespace Genetec.Dap.AccessControl
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEventRecordRepository
    {
        Task InsertRecords(IEnumerable<EventRecord> records);
    }
}