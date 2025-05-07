using DocumentFormat.OpenXml.Bibliography;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Extensions;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAUT_NCKH.Web.Controllers {
    [Route(RouterName.QUAN_LY_TAI_KHOAN)]
    [ServiceFilter(typeof(LayoutFilter))]
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

            // get previous filters
            string preSearch = HttpContext.Session.GetString("account_index_search")??"";
            int preRoleSelect = HttpContext.Session.GetInt32("account_index_roleId")??0;
            int preDepartSelect = HttpContext.Session.GetInt32("account_index_departmentId")??0;
            int prePageLength = HttpContext.Session.GetInt32("account_index_pageLength")??10;
            int prePageNumber = HttpContext.Session.GetInt32("account_index_pageNumber")??1;

            // re-applied filters if its the first request
            bool isFirstRequest = DepartmentId == 0 && RoleId == 0 && Search == "" && PageNumber == 1 && PageLength == 10;
            if (isFirstRequest) {
                DepartmentId = preDepartSelect;
                RoleId = preRoleSelect;
                Search = preSearch;
                PageLength = prePageLength;
                PageNumber = prePageNumber;
            }
            PageLength = prePageLength != PageLength ? PageLength : prePageLength;

            var viewModel = new AccountIndexViewPage(){
                Pagination = new DataTableOptions{
                    PageNumber = PageNumber,
                    PageLength = PageLength
                },
                RoleId = RoleId,
                DepartmentId = DepartmentId,
                Search = Search,
            };

            if (preSearch != viewModel.Search
                || preRoleSelect != viewModel.RoleId
                || preDepartSelect != viewModel.DepartmentId
                || prePageLength != viewModel.Pagination.PageLength) {
                viewModel.Pagination.PageNumber = 1;
            }

            var totalAccounts = await _accountService.GetCountDataTable(viewModel, userId);

            if (PageLength == totalAccounts) {
                viewModel.Pagination.PageNumber = 1;
            }
            var accounts = await _accountService.GetDataTable(viewModel, userId);
            var roles = await _categoryService.GetRoleListRoleBase(userId);
            var departments = await _categoryService.GetDepartmentListRoleBase(userId);
            var trainingPrograms = await _categoryService.GetTrainingProgramList();
            var majors = await _categoryService.GetMajorList();

            viewModel.Pagination.TotalRow = totalAccounts;
            viewModel.DataList = accounts;

            HttpContext.Session.SetString("account_index_search", viewModel.Search);
            HttpContext.Session.SetInt32("account_index_roleId", viewModel.RoleId);
            HttpContext.Session.SetInt32("account_index_departmentId", viewModel.DepartmentId);
            HttpContext.Session.SetInt32("account_index_pageLength", viewModel.Pagination.PageLength);
            HttpContext.Session.SetInt32("account_index_pageNumber", viewModel.Pagination.PageNumber);

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

        [HttpGet("search-teacher")]
        public async Task<IActionResult> SearchTeacher(string input, int departmentId) {
            var response = await _accountService.SearchTeacher(input, departmentId);
            return Ok(response);
        }
    }
}
