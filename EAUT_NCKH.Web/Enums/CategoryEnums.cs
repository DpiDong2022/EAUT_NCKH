namespace EAUT_NCKH.Web.Enums {
    public class RoleEnums {
        public const string STUDENT = "Sinh viên";
        public const string RESEARCH_ADVISOR = "Giảng viên hướng dẫn";
        public const string DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM = "Tổ Nghiên cứu Khoa học";
        public const string SCIENTIFIC_RESEARCH_OFFICE = "Phòng Nghiên cứu Khoa học";
    }

    public enum RoleEnumId {
        SCIENTIFIC_RESEARCH_OFFICE = 1,
        DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM,
        RESEARCH_ADVISOR,
        STUDENT
    }

    public class SexEnum {
        public const string MALE = "Nam";
        public const string FEMALE = "Nữ";
    }

    public enum SexEnumId {
        FEMALE,
        MALE
    }

    public class StudentRoleEnum {
        public const string MEMBER = "Thành viên";
        public const string LEADER = "Trưởng nhóm";
    }

    public enum StudentRoleEnumId {
        MEMBER,
        LEADER
    }

    public class TopicStatusEnums {
        public const string STUDENT = "Sinh viên";
        public const string RESEARCH_ADVISOR = "Giảng viên hướng dẫn";
        public const string DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM = "Tổ Nghiên cứu Khoa học";
        public const string SCIENTIFIC_RESEARCH_OFFICE = "Phòng Nghiên cứu Khoa học";
    }

    public enum TopicStatusEnumId {
        PENDING_REGISTRATION = 3,                         // Giảng viên đăng ký đề tài.
        WAITING_FOR_PROPOSAL_NOTICE = 4,                  // Tổ NCKH khoa gửi thông báo nộp thuyết minh.
        PROPOSAL_SUBMITTED = 5,                           // Sinh viên đã nộp file thuyết minh.
        PROPOSAL_REVIEW_ASSIGNMENT = 6,                   // Phân công hội đồng xét duyệt thuyết minh.
        PROPOSAL_APPROVED_BY_FACULTY = 7,                 // Hội đồng cấp khoa duyệt thuyết minh.
        PROPOSAL_APPROVED_BY_UNIVERSITY = 8,              // Phòng NCKH duyệt lần cuối thuyết minh.
        WAITING_FOR_FINAL_NOTICE = 9,                     // Thông báo yêu cầu nộp kết quả cuối cùng.
        FINAL_SUBMITTED = 10,                              // Sinh viên đã nộp kết quả đề tài.
        FINAL_REVIEW_ASSIGNMENT = 11,                      // Phân công hội đồng xét duyệt kết quả đề tài.
        FINAL_APPROVED_BY_FACULTY = 12,                   // Hội đồng cấp khoa xét duyệt kết quả đề tài.
        NOMINATED_FOR_UNIVERSITY_DEFENSE = 13,            // Cấp khoa đề xuất đề tài tiêu biểu.
        SELECTED_FOR_UNIVERSITY_DEFENSE = 14,             // Đề tài được chọn và phân công hội đồng bảo vệ cấp trường.
        COMPLETED = 15
    }
}
