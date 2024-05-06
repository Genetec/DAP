namespace Genetec.Dap.AccessControl
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEmployeeNumberReader
    {
        Task<HashSet<string>> GetAllEmployeeNumbers();
    }
}