using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EAUT_NCKH.Web.Controllers
{
    [Route("quan-ly-de-tai")]
    public class TopicController : Controller
    {
        private readonly TopicService _topicService;
        private readonly AuthService _authService;
        private readonly CategoryService _categoryService;

        public TopicController(TopicService topicService, AuthService authService, CategoryService category) {
            _topicService = topicService;
            _authService = authService;
            _categoryService = category;
        }

        public async Task<IActionResult> Index(int DepartmentId = -1, int Year = -1, int StatusId = -1, string SubStatusCode="-1", string Search = "", int PageNumber = 1, int PageLength = 10)
        {
            string token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            string role = _authService.GetRoleFromToken(token);
            int userId = _authService.GetAccountIdFromToken(token)??0;

            var paginationOptions = new TopicDataTableOptions(){
                Year = Year,
                Status = StatusId,
                SubStatus = SubStatusCode,
                DepartmentId = DepartmentId,
                Search = Search,
                PageNumber = PageNumber,
                PageLength = PageLength
            };

            string preSearch = HttpContext.Session.GetString("topic_index_search")??"";
            int preTopicStatusSelect = HttpContext.Session.GetInt32("topic_index_topicStatusId")??-1;
            int preYearSelect = HttpContext.Session.GetInt32("topic_index_year")??DateTime.Now.Year;
            string preSubStatusSelect = HttpContext.Session.GetString("topic_index_subStatusCode")??"";
            int preDepartSelect = HttpContext.Session.GetInt32("topic_index_departmentId")??-1;
            int prePageLength = HttpContext.Session.GetInt32("topic_index_pageLength")??-1;
            if (preSearch != paginationOptions.Search
                || preTopicStatusSelect != paginationOptions.Status
                || preYearSelect != paginationOptions.Year
                || preSubStatusSelect != paginationOptions.SubStatus
                || preDepartSelect != paginationOptions.DepartmentId
                || prePageLength != paginationOptions.PageLength) {
                paginationOptions.PageNumber = 1;
            }

            var totalTopics = await _topicService.GetCountDataTable(paginationOptions, userId);

            if (PageLength == totalTopics) {
                paginationOptions.PageNumber = 1;
            }

            var topics = await _topicService.GetDataTable(paginationOptions, userId);

            var topicStatusList = await _categoryService.GetTopicStatusList();
            var subStatusList = await _categoryService.GetSubStatusList();
            var departmentList = await _categoryService.GetDepartmentListRoleBase(userId);
            var trainingPrograms = await _categoryService.GetTrainingProgramList();
            var majors = await _categoryService.GetMajorListRoleBase(userId);

            paginationOptions.TotalRow = totalTopics;
            var viewModel = new TopicIndexViewPage(){
                Pagination = paginationOptions,
                Topics = topics,
                TopicStatuses = topicStatusList,
                Substatuses = subStatusList,
                Departments = departmentList,
                Trainingprograms = trainingPrograms,
                Majors = majors
            };

            HttpContext.Session.SetString("topic_index_search", paginationOptions.Search);
            HttpContext.Session.SetInt32("topic_index_year", paginationOptions.Year);
            HttpContext.Session.SetInt32("topic_index_topicStatusId", paginationOptions.Status);
            HttpContext.Session.SetString("topic_index_subStatusCode", paginationOptions.SubStatus);
            HttpContext.Session.SetInt32("topic_index_departmentId", paginationOptions.DepartmentId);
            HttpContext.Session.SetInt32("topic_index_pageLength", paginationOptions.PageLength);

            ViewBag.ReturnUrl = RouterName.QUAN_LY_DE_TAI;
            return View(viewModel);
        }

        [HttpGet("hai")]
        public async Task<IActionResult> Hai() {
            return View();
        }
    }
}
