namespace EAUT_NCKH.Web.DTOs {
    public class ResearchAdvisorViewModel : GeneralInformationAccount {
        public int? majorId { get; set; }
        public string? academicTitle { get; set; }
    }
    public class GeneralInformationAccount {
        public int roleId { get; set; }
        public int id { get; set; }
        public string fullName { get; set; } = string.Empty;
        public string? phoneNumber { get; set; }
        public string? email { get; set; }
        public int? departmentId { get; set; }
    }
}
