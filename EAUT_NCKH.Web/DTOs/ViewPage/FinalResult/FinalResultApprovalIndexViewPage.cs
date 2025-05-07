using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.DTOs {
    public class FinalResultApprovalIndexViewPage: IndexViewPage<TopicModelView> {
        public int DepartmentId { get; set; } = -1;
        public int StatusId { get; set; } = -1;
        public List<Department> Departments { get; set; }
        public List<Topicstatus> TopicStatuses { get; set; }
        public List<Substatus> Substatuses { get; set; }
        public List<Criteriaevaluationtype> Criteriaevaluations { get; set; }
    }
}
