using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;

namespace EAUT_NCKH.Web.Services {
    public class StudentService {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository) {
            _studentRepository = studentRepository;
        }

        public async Task<double> GetCountDataTable(StudentIndexViewPage options, int senderId) {
            return await _studentRepository.GetCountDataTable(options, senderId);
        }

        public async Task<List<Student>>? GetDataTable(StudentIndexViewPage options, int senderId) {
            return await _studentRepository.GetDataTable(options, senderId);
        }
    }
}
