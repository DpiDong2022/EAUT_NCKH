using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Extensions;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EAUT_NCKH.Web.DTOs.Request;

namespace EAUT_NCKH.Web.Controllers
{
    [Route(RouterName.PHAN_CONG_BAO_VE_DE_TAI)]
    [ServiceFilter(typeof(LayoutFilter))]
    [Authorize(Roles = RoleEnums.SCIENTIFIC_RESEARCH_OFFICE)]
    public class TopicDefenseController : Controller
    {
        private readonly TopicDefenseService _topicDeService;
        private readonly AuthService _authService;
        private readonly CategoryService _categoryService;
        private readonly IConfiguration _configuration;

        public TopicDefenseController(TopicDefenseService topicService, AuthService authService, CategoryService category,
            IConfiguration configuration) {
            _authService = authService;
            _categoryService = category;
            _configuration = configuration;
            _topicDeService = topicService;
        }

        public async Task<IActionResult> Index(int DepartmentId = -1, int Year = -1, int StatusId = -1, int PageNumber = 1, int PageLength = 10) {
            string token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            string role = _authService.GetRoleFromToken(token);
            int userId = _authService.GetAccountIdFromToken(token)??0;

            if (Year == -1) {
                Year = DateTime.Now.Year;
            }
            // get previous filter
            int preTopicStatusSelect = HttpContext.Session.GetInt32("topicdefense_index_topicStatusId")??-1;
            int preDepartSelect = HttpContext.Session.GetInt32("topicdefense_index_departmentId")??-1;
            int prePageLength = HttpContext.Session.GetInt32("topicdefense_index_pageLength")??10;
            int prePageNumber = HttpContext.Session.GetInt32("topicdefense_index_pageNumber")??1;
            int preYearSelect= HttpContext.Session.GetInt32("topicdefense_index_year")??DateTime.Now.Year;

            // re-applied filters if its the first request
            bool isFirstRequest = DepartmentId == -1 && StatusId == 3 && PageNumber == 1 && PageLength == 10 && Year == DateTime.Now.Year;
            if (isFirstRequest) {
                DepartmentId = preDepartSelect;
                StatusId = preTopicStatusSelect;
                PageLength = prePageLength;
                Year = preYearSelect;
            }
            PageLength = prePageLength != PageLength ? PageLength : prePageLength;

            // get select list
            var topicStatusList = await _categoryService.GetTopicStatusList();

            var minStatus = (int)TopicStatusEnumId.NOMINATED_FOR_UNIVERSITY_DEFENSE;

            topicStatusList = topicStatusList.Where(c => c.Id >= minStatus).ToList();
            var departmentList = await _categoryService.GetDepartmentListRoleBase(userId);
            var majors = await _categoryService.GetMajorListRoleBase(userId);
            var buildings = await _categoryService.GetAllBuildings();
            var rooms = await _categoryService.GetAllRooms();

            var viewModel = new TopicDefenseIndexViewPage {
                Pagination = new DataTableOptions{
                    PageNumber = PageNumber,
                    PageLength = PageLength
                },
                Status = StatusId,
                DepartmentId = DepartmentId,
                Year = Year,

                TopicStatuses = topicStatusList,
                Departments = departmentList,
                Majors = majors,
                Buildings = buildings,
                Rooms = rooms
            };

            // check to reset page number
            if (preTopicStatusSelect != viewModel.Status
                || preDepartSelect != viewModel.DepartmentId
                || prePageLength != viewModel.Pagination.PageLength) {
                viewModel.Pagination.PageNumber = 1;
            }

            var totalTopics = await _topicDeService.GetCountDataTable(userId, viewModel);

            if (PageLength == totalTopics) {
                viewModel.Pagination.PageNumber = 1;
            }

            var topics = await _topicDeService.GetDataTable(userId, viewModel);

            viewModel.Pagination.TotalRow = totalTopics;
            viewModel.DataList = topics;

            // save previous filters
            HttpContext.Session.SetInt32("topicdefense_index_topicStatusId", viewModel.Status);
            HttpContext.Session.SetInt32("topicdefense_index_departmentId", viewModel.DepartmentId);
            HttpContext.Session.SetInt32("topicdefense_index_pageLength", viewModel.Pagination.PageLength);
            HttpContext.Session.SetInt32("topicdefense_index_pageNumber", viewModel.Pagination.PageNumber);
            HttpContext.Session.SetInt32("topicdefense_index_year", viewModel.Year);

            var allDepartments = await _categoryService.GetAllDepartmentAsync();
            ViewBag.ReturnUrl = RouterName.PHAN_CONG_BAO_VE_DE_TAI;
            ViewBag.AllDepartments = allDepartments;
            return View(viewModel);
        }

        [HttpPost("modify-committee-defense")]
        [Authorize(Roles = RoleEnums.SCIENTIFIC_RESEARCH_OFFICE)]
        public async Task<IActionResult> ModifyCommitteeDefense(CommitteeAddRequest request) {

            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");

            var data = await _topicDeService.AddOrEditCommitteeAssignment(senderId??0, request);
            return Ok(data);
        }

        [HttpPost("modify-score")]
        [Authorize(Roles = RoleEnums.SCIENTIFIC_RESEARCH_OFFICE)]
        public async Task<IActionResult> ModifyScore(int score, string topicId, IFormFile files) {

            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");

            var data = await _topicDeService.ModifyScore(senderId??0, topicId, score, files);

            TempData["code"] = data.code;
            TempData["message"] = data.message;
            return RedirectToAction("Index");
        }
    }
}
