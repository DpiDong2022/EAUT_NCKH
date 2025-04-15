using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EAUT_NCKH.Web.Controllers.Components {
    public class SidebarViewComponent : ViewComponent {
        private readonly AuthService _authService;

        public SidebarViewComponent(AuthService authService) {
            _authService = authService;
        }

        public async Task<IViewComponentResult> InvokeAsync() {
            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var RoleName = _authService.GetRoleFromToken(token);

            var MenuItems = new List<MenuItem>() {
                new MenuItem() {Title = "Trang chủ", Url="/trang-chu", SubItems = new List<MenuItem>() {
                    //new MenuItem() { Title="Luật lệ", Url="/trang-chu/thu-gi-khac" }
                }}
            };

            if (RoleName == RoleEnums.STUDENT) {
                MenuItems.Add(new MenuItem() { Title = "Đề tài của tôi", Url = "/my-topic" });
            }

            else if (RoleName == RoleEnums.RESEARCH_ADVISOR) {
                MenuItems.Add(new MenuItem() { Title = "Quản lý Đề tài", Url = "/quan-ly-de-tai" });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Thuyết minh", SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Danh sách Phân công Phê duyệt", Url = "/ds-phan-cong-phe-duyet-thuyet-minh" }
                }});
                MenuItems.Add(new MenuItem() { Title = "Quản lý Phê duyệt Đề tài", SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Danh sách Phân công Xét duyệt", Url = "/ds-phan-cong-xet-duyet-de-tai" }}
                });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Chấm điểm Đề tài", SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Danh sách Phân công Chấm điểm", Url = "/ds-phan-cong-cham-diem-de-tai" }}
                });
            }

            if (RoleName == RoleEnums.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM) {
                MenuItems.Add(new MenuItem() { Title = "Quản lý Tài khoản", Url = "/ql-tai-khoan" });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Đề tài", Url = "/quan-ly-de-tai" });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Thuyết minh", SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Phân công Hội đồng Phê duyệt", Url = "/phan-cong-phe-duyet-thuyet-minh" },
                    new MenuItem () { Title = "Danh sách Phân công Phê duyệt", Url = "/ds-phan-cong-phe-duyet-thuyet-minh" }}
                });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Hội đồng", Url = "/quan-ly-hoi-dong" });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Phê duyệt Đề tài", SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Phân công Hội đồng Xét duyệt", Url = "/phan-cong-xet-duyet-de-tai" },
                    new MenuItem () { Title = "Danh sách Phân công Xét duyệt", Url = "/ds-phan-cong-xet-duyet-de-tai" }}
                });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Chấm điểm Đề tài", SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Phân công Hội đồng Chấm điểm", Url = "/phan-cong-cham-diem-de-tai" },
                    new MenuItem () { Title = "Danh sách Phân công Chấm điểm", Url = "/ds-phan-cong-cham-diem-de-tai" }}
                });
            }
            if (RoleName == RoleEnums.SCIENTIFIC_RESEARCH_OFFICE) {
                MenuItems.Add(new MenuItem() { Title = "Quản lý Tài khoản", Url = "/ql-tai-khoan" });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Đề tài", Url = "/quan-ly-de-tai" });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Thuyết minh", SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Danh sách Phân công Phê duyệt", Url = "/ds-phan-cong-phe-duyet-thuyet-minh" }}
                });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Hội đồng", Url = "/quan-ly-hoi-dong" });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Phê duyệt Đề tài", SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Danh sách Phân công Xét duyệt", Url = "/ds-phan-cong-xet-duyet-de-tai" }}
                });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Chấm điểm Đề tài", SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Phân công Hội đồng Chấm điểm", Url = "/phan-cong-cham-diem-de-tai" },
                    new MenuItem () { Title = "Danh sách Phân công Chấm điểm", Url = "/ds-phan-cong-cham-diem-de-tai" }}
                });
            }

            return View(MenuItems);
        }
    }
}
