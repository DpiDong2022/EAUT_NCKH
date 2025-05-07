using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Models;
using Org.BouncyCastle.Asn1.Ocsp;

namespace EAUT_NCKH.Web.Repositories.IRepositories
{
    public interface ICommitteeRepository
    {
        Task<ResponseData<List<Committee>>> GetDataTable(int senderId, CommitteeIndexViewPage option);
        double GetDataTableCount(int senderId, CommitteeIndexViewPage option);
        Task<List<Committeetype>> GetCommitteeTypes();
        ResponseData<Committee> GetProposalCommittee(int senderId, string encodedTopicId);
        ResponseData<Committee> GetFinalCommittee(int senderId, string encodedTopicId);
        ResponseData<Committee> GetDefenseCommittee(int senderId, string encodedTopicId);
    }
}
