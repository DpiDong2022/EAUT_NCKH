using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EAUT_NCKH.Web.Controllers {
    [Route("ql-tai-khoan")]
    [Authorize(Roles = RoleEnums.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM +","+ RoleEnums.SCIENTIFIC_RESEARCH_OFFICE)]
    public class AccountController : Controller {

        private readonly AccountService _accountService;
        private readonly CategoryService _categoryService;
        private readonly AuthService _authService;
        public AccountController(
            AccountService accountService,
            CategoryService categoryService,
            AuthService authService
        ) {
            _accountService = accountService;
            _categoryService = categoryService;
            _authService = authService;
        }

        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int RoleId = 0, int DepartmentId = 0, string Search = "", int PageNumber = 1, int PageLength = 10) {

            string token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            string role = _authService.GetRoleFromToken(token);
            int userId = _authService.GetAccountIdFromToken(token)??0;

            var paginationOptions = new AccountDataTableOptions(){
                RoleId = RoleId,
                DepartmentId = DepartmentId,
                Search = Search,
                PageNumber = PageNumber,
                PageLength = PageLength
            };

            string preSearch = HttpContext.Session.GetString("account_index_search")??"";
            int preRoleSelect = HttpContext.Session.GetInt32("account_index_roleId")??0;
            int preDepartSelect = HttpContext.Session.GetInt32("account_index_departmentId")??0;
            int prePageLength = HttpContext.Session.GetInt32("account_index_pageLength")??0;
            if (preSearch != paginationOptions.Search
                || preRoleSelect != paginationOptions.RoleId
                || preDepartSelect != paginationOptions.DepartmentId
                || prePageLength != paginationOptions.PageLength) {
                paginationOptions.PageNumber = 1;
            }

            var totalAccounts = await _accountService.GetCountDataTable(paginationOptions, userId);

            if (PageLength == totalAccounts) {
                paginationOptions.PageNumber = 1;
            }
            var accounts = await _accountService.GetDataTable(paginationOptions, userId);
            var roles = await _categoryService.GetRoleListRoleBase(userId);
            var departments = await _categoryService.GetDepartmentListRoleBase(userId);
            var trainingPrograms = await _categoryService.GetTrainingProgramList();
            var majors = await _categoryService.GetMajorList();

            HttpContext.Session.SetString("account_index_search", paginationOptions.Search);
            HttpContext.Session.SetInt32("account_index_roleId", paginationOptions.RoleId);
            HttpContext.Session.SetInt32("account_index_departmentId", paginationOptions.DepartmentId);
            HttpContext.Session.SetInt32("account_index_pageLength", paginationOptions.PageLength);

            paginationOptions.TotalRow = totalAccounts;
            var viewModel = new AccountIndexViewPage(){
                Pagination = paginationOptions,
                Accounts = accounts
            };

            ViewBag.Roles = roles;
            ViewBag.Departments = departments;
            ViewBag.TrainingPrograms = trainingPrograms;
            ViewBag.Majors = majors;
            ViewBag.ReturnUrl = RouterName.QUAN_LY_TAI_KHOAN;
            return View(viewModel);
        }

        [HttpPost("modify-acb-student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Student(StudentViewModel viewModel) {
            string token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            int createrId = _authService.GetAccountIdFromToken(token)??0;
            if (createrId == 0) {
                return Ok(new Response() { code = 1, message = "Bạn không có quyền thêm tài khoản" });
            }
            var response = await _accountService.AddOrEditStudent(viewModel, createrId);
            return Ok(response);
        }

        [HttpPost("modify-abc-teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Teacher
            (ResearchAdvisorViewModel viewModel) 
        {
            string token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            int createrId = _authService.GetAccountIdFromToken(token)??0;
            if (createrId == 0) {
                return Ok(new Response() { code = 1, message ="Bạn không có quyền thêm tài khoản" });
            }
            var response = await _accountService.AddOrEditTeacher(viewModel, createrId);
            return Ok(response);
        }

        [HttpPost("modify-abc-tonckh")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToNCKH(GeneralInformationAccount viewModel) {
            var response = await _accountService.AddOrEditToNCKH(viewModel);
            return Ok(response);
        }

        [HttpGet("get-account-information")]
        public async Task<IActionResult> GetAccountInformation(int id) {
            var response = await _accountService.GetAccountInformation(id);
            return Ok(response);
        }
    }
}
