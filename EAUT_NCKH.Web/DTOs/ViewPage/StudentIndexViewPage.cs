using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.DTOs {
    public class StudentIndexViewPage : IndexViewPage<Student> {
        public List<Department> Departments { get; set; } = new List<Department>();
        public List<Trainingprogram> Trainingprograms { get; set; } = new List<Trainingprogram>();
        public List<Major> Majors { get; set; } = new List<Major>();

        public int TrainingProgramId { get; set; } = -1;
        public int DepartmentId { get; set; } = -1;
        public int MajorId { get; set; } = -1;
        public string Search { get; set; } = "";
    }
}
