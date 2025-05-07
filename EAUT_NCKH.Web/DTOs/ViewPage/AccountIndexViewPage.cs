using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.DTOs {

    public class AccountIndexViewPage: IndexViewPage<Account> {
        public int RoleId { get; set; } = 0;
        public int DepartmentId { get; set; } = 0;
        public string Search { get; set; } = "";
    }
}
