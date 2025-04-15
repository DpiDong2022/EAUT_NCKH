using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;

namespace EAUT_NCKH.Web.Services {
    public class CommitteeService {
        private readonly ICommitteeRepository _committeeRepository;

        public CommitteeService(ICommitteeRepository committeeRepository) {
            _committeeRepository = committeeRepository;
        }

        public async Task<List<Committee>> ViewCommittees() {
            return await _committeeRepository.GetCommittees();
        }

        public async Task<List<Committeetype>> ViewCommitteeTypes() {
            return await _committeeRepository.GetCommitteeTypes();
        }
    }
}
