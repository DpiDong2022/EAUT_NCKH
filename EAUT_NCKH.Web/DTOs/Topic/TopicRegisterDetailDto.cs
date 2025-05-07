using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.DTOs {
    public class TopicRegisterDetailDto {
        public string EncyptedID { get; set; }
        public string Note { get; set; }
        public string Title { get; set; }
        public Account SecondteacherAccount { get; set; }
        public List<Topicstudent> StudentList { get; set; }
    }

    public class TopicStudentRegisterdDto {

    }
}
