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

    public enum ProposalEvaluationTypeId {
        COMMITTEE_EVALUATION, 
        SR_EVALUATION  
    }

    public enum ProposalStatusId {
        APPROVED = 1,
        NEED_REVISION,
        REJECTED
    }
    public class ProposalStatusEnums {
        public const string APPROVED = "Được duyệt";
        public const string NEED_REVISION = "Được duyệt (Cần sửa đổi theo phản hồi)";
        public const string REJECTED = "Không được duyệt";
    }

    public enum NotificationType {
        REQUEST_SUBMIT_PROPOSAL = 1,  // Yêu cầu nộp thuyết minh
        UPDATE_DEADLINE_SUBMIT_PROPOSAL = 2,         // Cập nhật hạn nộp thuyết minh
        SUBMIT_PROPOSAL = 3,                   // Đề tài đã nộp thuyết minh
        UPDATE_PROPOSAL = 4,                    // Cập nhật thuyết minh đề tài
        PROPOSAL_ASSIGNMENT = 5,
        UPDATE_PROPOSAL_ASSIGNMENT = 6,
        UPDATE_MEMBER_PROPOSAL_ASSIGNMENT = 9,
        APPROVE_PROPOSAL_BY_COMMITTEE = 10,             // Hội đồng phê duyệt thuyết minh
        APPROVE_PROPOSAL_BY_RESEARCH_OFFICE = 12,       // Phòng NCKH phê duyệt thuyết minh
        CANCEL_APPROVAL_PROPOSAL = 13,                  // hủy phê duyệt thuyết minh
        REQUEST_SUBMIT_FINAL_RESULT = 14,               // Yêu cầu nộp báo cáo
        SUBMIT_FINAL = 15,

        FINAL_RESULT_ASSIGNMENT = 16,
        UPDATE_FINAL_RESULT_ASSIGNMENT = 17,
        UPDATE_MEMBER_FINAL_RESULT_ASSIGNMENT = 18,
        FINAL_RESULT_APPROVED = 19,
        NOMINATE_FOR_UNIVERSITY_DEFENSE = 21,
        DEFENSE_ASSIGNMENT = 23,
        UPDATE_DEFENSE_ASSIGNMENT = 25,
    }

    public enum CommitteeTypeEnumId {
        PROPOSAL_COMMITTEE = 1,
        FINAL_COMMITTEE = 2,
        DEFENSE_COMMITTEE = 3,
    }

    public enum CommitteeRoleEnumId {
        PROPOSAL_CHU_TICH_HD = 1,
        PROPOSAL_PHAN_BIEN,
        PROPOSAL_THU_KY_HD,

        FINAL_RESULT_CHU_TICH_HD = 4,
        FINAL_RESULT_THU_KY_HD = 5,
        FINAL_RESULT_PHAN_BIEN = 6,
        FINAL_RESULT_UY_VIEN = 7,
        FINAL_RESULT_PBDL = 8,

        DEFENSE_CHU_TICH_HD = 9,
        DEFENSE_RESULT_THU_KY_HD = 10,
        DEFENSE_RESULT_PHAN_BIEN = 11,
        DEFENSE_RESULT_UY_VIEN = 12,
        DEFENSE_RESULT_PBDL = 13,
    }
}
