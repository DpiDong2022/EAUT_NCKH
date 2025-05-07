using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Extensions;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAUT_NCKH.Web.Controllers {
    [Route(RouterName.QUAN_LY_TT_SINH_VIEN)]
    [Authorize(Roles = RoleEnums.SCIENTIFIC_RESEARCH_OFFICE)]
    [ServiceFilter(typeof(LayoutFilter))]
    public class StudentController: Controller {
        private readonly StudentService _studentService;
        private readonly CategoryService _categoryService;
        private readonly AuthService _authService;

        public StudentController(StudentService studentService, CategoryService categoryService, AuthService authService) {
            _studentService = studentService;
            _categoryService = categoryService;
            _authService = authService;
        }

        public async Task<IActionResult> Index(int DepartmentId = -1, int MajorId = -1, string search = "", 
            int PageNumber = 1, int PageLength = 10,
            int TrainingProgramId = -1, int Year = -1, string Search = "") {

            string token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            string role = _authService.GetRoleFromToken(token);
            int userId = _authService.GetAccountIdFromToken(token)??0;

            // get previous filters
            string preSearch = HttpContext.Session.GetString("student_index_search")??"";
            int preDepartSelect = HttpContext.Session.GetInt32("student_index_departmentId")??-1;
            int preMajorSelect = HttpContext.Session.GetInt32("student_index_majorId")??-1;
            int pretrainingProgramSelect = HttpContext.Session.GetInt32("student_index_trainingProgramId")??-1;
            int prePageLength = HttpContext.Session.GetInt32("student_index_pageLength")??10;
            int prePageNumber = HttpContext.Session.GetInt32("student_index_pageNumber")??1;

            // re-applied filters if its the first request
            bool isFirstRequest = DepartmentId == -1 && MajorId == -1 && TrainingProgramId == -1 && Search == "" && PageNumber == 1 && PageLength == 10;
            if (isFirstRequest) {
                DepartmentId = preDepartSelect;
                MajorId = preMajorSelect;
                TrainingProgramId = pretrainingProgramSelect;
                Search = preSearch;
                PageNumber = prePageNumber;
                PageLength = prePageLength;
            }
            PageLength = prePageLength != PageLength ? PageLength : prePageLength;

            var subStatusList = await _categoryService.GetSubStatusList();
            var departmentList = await _categoryService.GetDepartmentListRoleBase(userId);
            var trainingPrograms = await _categoryService.GetTrainingProgramList();
            var majors = await _categoryService.GetMajorListRoleBase(userId);

            var viewModel = new StudentIndexViewPage(){
                Pagination = new DataTableOptions{
                    PageNumber = PageNumber,
                    PageLength = PageLength
                },
                DepartmentId = DepartmentId,
                MajorId = MajorId,
                TrainingProgramId = TrainingProgramId,
                Search = Search,

                Departments = departmentList,
                Trainingprograms = trainingPrograms,
                Majors = majors
            };

            if (viewModel.Search == null) {
                viewModel.Search = "";
            }
            if (preSearch != viewModel.Search
                || preDepartSelect != viewModel.DepartmentId
                || preMajorSelect != viewModel.MajorId
                || pretrainingProgramSelect != viewModel.TrainingProgramId
                || prePageLength != viewModel.Pagination.PageLength) {
                viewModel.Pagination.PageNumber = 1;
            }

            var totalStudents = await _studentService.GetCountDataTable(viewModel, userId);

            if (viewModel.Pagination.PageLength == totalStudents) {
                viewModel.Pagination.PageNumber = 1;
            }

            var students = await _studentService.GetDataTable(viewModel, userId);

            var topicStatusList = await _categoryService.GetTopicStatusList();

            viewModel.Pagination.TotalRow = totalStudents;
            viewModel.DataList = students;

            HttpContext.Session.SetString("student_index_search", viewModel.Search);
            HttpContext.Session.SetInt32("student_index_departmentId", viewModel.DepartmentId);
            HttpContext.Session.SetInt32("student_index_majorId", viewModel.MajorId);
            HttpContext.Session.SetInt32("student_index_trainingProgramId", viewModel.TrainingProgramId);
            HttpContext.Session.SetInt32("student_index_pageLength", viewModel.Pagination.PageLength);
            HttpContext.Session.SetInt32("student_index_pageNumber", viewModel.Pagination.PageNumber);

            ViewBag.ReturnUrl = RouterName.QUAN_LY_TT_SINH_VIEN;
            return View(viewModel);
        }
    }
}
