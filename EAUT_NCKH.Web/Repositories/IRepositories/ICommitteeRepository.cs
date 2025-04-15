using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.Repositories.IRepositories
{
    public interface ICommitteeRepository
    {
        Task<List<Committee>> GetCommittees();
        Task<List<Committeetype>> GetCommitteeTypes();
    }
}
