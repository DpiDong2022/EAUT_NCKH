using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.Repositories.IRepositories {
    public interface IStudentRepository {
        //public Student ViewProfileByAccountId(int accountId);
        //public Student ViewProfileByStudentId(int studentId);
        Task<List<Student>> GetDataTable(StudentIndexViewPage options, int senderId);
        Task<double> GetCountDataTable(StudentIndexViewPage options, int senderId);
    }
}
