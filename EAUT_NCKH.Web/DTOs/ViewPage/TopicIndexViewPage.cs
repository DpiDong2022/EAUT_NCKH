using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.DTOs {
    public class TopicIndexViewPage: IndexViewPage<TopicModelView> {
        public List<Department> Departments { get; set; } = new List<Department>();
        public List<Topicstatus> TopicStatuses { get; set; } = new List<Topicstatus>();
        public List<Trainingprogram> Trainingprograms { get; set; } = new List<Trainingprogram>();
        public List<Major> Majors { get; set; } = new List<Major>();
        public List<Building> Buildings { get; set; } = new List<Building>();
        public List<Room> Rooms { get; set; } = new List<Room>();

        public int DepartmentId { get; set; } = -1;
        public string Search { get; set; } = "";
        public int Status { get; set; } = -1;
        public int Year { get; set; } = DateTime.Now.Year;
        public int TopicId { get; set; } = -1;
    }
}
