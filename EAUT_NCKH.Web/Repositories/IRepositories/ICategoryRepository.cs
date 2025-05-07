using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.Repositories.IRepositories {
    public interface ICategoryRepository<T> {
        Task<List<T>> GetValueList();
        Task<List<Department>> GetDepartmentListRoleBase(int userId);
        Task<List<Role>> GetRoleListRoleBase(int userId);
        Task<List<Major>> GetMajorListRoleBase(int userId);
        Task<List<Department>> GetAllDepartment(string search);
        Task<List<Building>> GetAllBuildings();
        Task<List<Room>> GetAllRooms(int buildingId);
        Task<List<Criteriaevaluationtype>> GetListOfCriteria();
    }
}
