namespace EAUT_NCKH.Web.DTOs {
    public class TopicStudentDto {
        public int? Id { get; set; } = 0;
        public string StudentCode { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public string ClassName { get; set; }
        public int TrainingProgramId { get; set; }
        public int DepartmentId { get; set; }
        public int MajorId { get; set; }
    }
}
