using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using Org.BouncyCastle.Asn1.Ocsp;

namespace EAUT_NCKH.Web.Services {
    public class CommitteeService {
        private readonly ICommitteeRepository _committeeRepository;

        public CommitteeService(ICommitteeRepository committeeRepository) {
            _committeeRepository = committeeRepository;
        }

        public async Task<ResponseData<List<Committee>>> GetDataTable(int senderId, CommitteeIndexViewPage options) {
            return await _committeeRepository.GetDataTable(senderId, options);
        }

        public double GetDataTableCount(int senderId, CommitteeIndexViewPage options) {
            return _committeeRepository.GetDataTableCount(senderId, options);
        }

        public async Task<List<Committeetype>> ViewCommitteeTypes() {
            return await _committeeRepository.GetCommitteeTypes();
        }

        public ResponseData<Committee> GetProposalCommittee(int senderId, string encodedTopicId) {
            return _committeeRepository.GetProposalCommittee(senderId, encodedTopicId);
        }

        public ResponseData<Committee> GetFinalCommittee(int senderId, string encodedTopicId) {
            return _committeeRepository.GetFinalCommittee(senderId, encodedTopicId);
        }

        public ResponseData<Committee> GetDefenseCommittee(int senderId, string encodedTopicId) {
            return _committeeRepository.GetDefenseCommittee(senderId, encodedTopicId);
        }
    }

}
