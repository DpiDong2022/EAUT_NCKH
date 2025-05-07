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
                MenuItems.Add(new MenuItem() { Title = "Quản lý Đề tài", Url = RouterName.QUAN_LY_DE_TAI });
            }

            else if (RoleName == RoleEnums.RESEARCH_ADVISOR) {
                MenuItems.Add(new MenuItem() { Title = "Quản lý Đề tài", Url = RouterName.QUAN_LY_DE_TAI });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Thuyết minh", 
                    Url = RouterName.PHE_DUYET_THUYET_MINH,
                    SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Phê duyệt thuyết minh", Url = RouterName.PHE_DUYET_THUYET_MINH }
                }});
                MenuItems.Add(new MenuItem() { Title = "Quản lý Đánh giá Đề tài",
                    Url = RouterName.PHAN_CONG_PHE_DUYET_FINAL_RESULT + RouterName.PHE_DUYET_FINAL_RESULT,
                    SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Danh sách Phân công Đánh giá", Url = RouterName.PHE_DUYET_FINAL_RESULT }}
                });
            }

            if (RoleName == RoleEnums.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM) {
                MenuItems.Add(new MenuItem() { Title = "Quản lý Tài khoản", Url = RouterName.QUAN_LY_TAI_KHOAN });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Đề tài", Url = RouterName.QUAN_LY_DE_TAI });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Thuyết minh", 
                    Url = RouterName.PHE_DUYET_THUYET_MINH + RouterName.PHAN_CONG_PHE_DUYET_TM,
                    SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Phân công Hội đồng", Url = RouterName.PHAN_CONG_PHE_DUYET_TM },
                    new MenuItem () { Title = "Phê duyệt thuyết minh", Url = RouterName.PHE_DUYET_THUYET_MINH }}
                });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Hội đồng", Url = RouterName.QUAN_LY_HOI_DONG });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Đánh giá Đề tài",
                    Url = RouterName.PHAN_CONG_PHE_DUYET_FINAL_RESULT + RouterName.PHE_DUYET_FINAL_RESULT,
                    SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Phân công Hội đồng đánh giá", Url = RouterName.PHAN_CONG_PHE_DUYET_FINAL_RESULT },
                    new MenuItem () { Title = "Danh sách Phân công Đánh giá", Url = RouterName.PHE_DUYET_FINAL_RESULT }}
                });
            }
            if (RoleName == RoleEnums.SCIENTIFIC_RESEARCH_OFFICE) {
                MenuItems.Add(new MenuItem() { Title = "Quản lý Tài khoản", Url = RouterName.QUAN_LY_TAI_KHOAN });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Thông tin Sinh viên", Url = RouterName.QUAN_LY_TT_SINH_VIEN });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Đề tài", Url = RouterName.QUAN_LY_DE_TAI });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Thuyết minh",
                    Url = RouterName.PHE_DUYET_THUYET_MINH, 
                    SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Phê duyệt thuyết minh", Url = RouterName.PHE_DUYET_THUYET_MINH }}
                });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Hội đồng", Url = RouterName.QUAN_LY_HOI_DONG });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Đánh giá Đề tài",
                    Url = RouterName.PHAN_CONG_PHE_DUYET_FINAL_RESULT + RouterName.PHE_DUYET_FINAL_RESULT,
                    SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Danh sách Phân công Đánh giá", Url = RouterName.PHE_DUYET_FINAL_RESULT }}
                });
                MenuItems.Add(new MenuItem() { Title = "Quản lý Chấm điểm Đề tài",
                    Url = RouterName.PHAN_CONG_BAO_VE_DE_TAI,
                    SubItems = new List<MenuItem>() {
                    new MenuItem () { Title = "Phân công Hội đồng Chấm điểm", Url = RouterName.PHAN_CONG_BAO_VE_DE_TAI }}
                });
            }

            return View(MenuItems);
        }
    }
}
