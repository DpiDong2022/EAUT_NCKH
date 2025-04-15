using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.DTOs {
    public class TopicIndexViewPage : ViewPage {
        public TopicDataTableOptions Pagination { get; set; } = new TopicDataTableOptions();
        public List<Topic> Topics { get; set; } = new List<Topic>();
        public List<Department> Departments { get; set; } = new List<Department>();
        public List<Topicstatus> TopicStatuses { get; set; } = new List<Topicstatus>();
        public List<Substatus> Substatuses { get; set; } = new List<Substatus>();
        public List<Trainingprogram> Trainingprograms { get; set; } = new List<Trainingprogram>();
        public List<Major> Majors { get; set; } = new List<Major>();
        public override void CalculatePagination() {
            base.CalculatePagination();
            if (Pagination != null) {
                Start = Math.Max(1, Pagination.PageNumber - Range);
                End = Math.Min(Pagination.GetTotalPage(), Pagination.PageNumber + Range);
                if (Topics.Count == 0) {
                    Pagination.PageNumber = 0;
                }
            }
        }
    }
}
