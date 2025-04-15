namespace EAUT_NCKH.Web.DTOs {
    public class StudentViewModel : GeneralInformationAccount {
        public int? trainingProgram { get; set; }
        public int? majorId { get; set; } = 0;
        public string? className { get; set; }
        public int? studentCode { get; set; } = 0;
    }
}
