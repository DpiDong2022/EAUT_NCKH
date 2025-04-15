using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using Microsoft.AspNetCore.Identity;

namespace EAUT_NCKH.Web.Services {
    public class CategoryService {
        private readonly ICategoryRepository<Trainingprogram> _TPRepository;
        private readonly ICategoryRepository<Major> _majorRepository;
        private readonly ICategoryRepository<Role> _roleRepository;
        private readonly ICategoryRepository<Department> _departRepository;
        private readonly ICategoryRepository<Topicstatus> _topicStatusRepository;
        private readonly ICategoryRepository<Substatus> _subStatusRepository;
        public CategoryService(
            ICategoryRepository<Trainingprogram> TPRepository,
            ICategoryRepository<Major> majorRepository,
            ICategoryRepository<Role> roleRepository,
            ICategoryRepository<Department> departRepository,
            ICategoryRepository<Topicstatus> topicStatusRepository,
            ICategoryRepository<Substatus> subStatusRepository
        ) {
            _TPRepository = TPRepository;
            _majorRepository = majorRepository;
            _roleRepository = roleRepository;
            _departRepository = departRepository;
            _topicStatusRepository = topicStatusRepository;
            _subStatusRepository = subStatusRepository;
        }

        public async Task<List<Trainingprogram>> GetTrainingProgramList() {
            return await _TPRepository.GetValueList();
        }

        public async Task<List<Major>> GetMajorList() {
            return await _majorRepository.GetValueList();
        }

        public async Task<List<Major>> GetMajorListRoleBase(int userId) {
            return await _majorRepository.GetMajorListRoleBase(userId);
        }

        public async Task<List<Role>> GetRoleList() {
            return await _roleRepository.GetValueList();
        }

        public async Task<List<Department>> GetDepartmentList() {
            return await _departRepository.GetValueList();
        }

        public async Task<List<Department>> GetDepartmentListRoleBase(int userId) {
            return await _departRepository.GetDepartmentListRoleBase(userId);
        }

        public async Task<List<Role>> GetRoleListRoleBase(int userId) {
            return await _departRepository.GetRoleListRoleBase(userId);
        }

        public async Task<List<Topicstatus>> GetTopicStatusList() {
            return await _topicStatusRepository.GetValueList();
        }

        public async Task<List<Substatus>> GetSubStatusList() {
            return await _subStatusRepository.GetValueList();
        }
    }
}
