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
}
