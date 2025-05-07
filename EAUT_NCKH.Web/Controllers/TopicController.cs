using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Extensions;
using EAUT_NCKH.Web.Services;
using HashidsNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAUT_NCKH.Web.Controllers
{
    [Route(RouterName.QUAN_LY_DE_TAI)]
    [ServiceFilter(typeof(LayoutFilter))]
    [Authorize]
    public class TopicController : Controller
    {
        private readonly TopicService _topicService;
        private readonly AuthService _authService;
        private readonly CategoryService _categoryService;
        private readonly IConfiguration _configuration;
        private readonly ReportService _reportService;

        public TopicController(TopicService topicService, AuthService authService, CategoryService category, 
            IConfiguration configuration, ReportService reportService) {
            _topicService = topicService;
            _authService = authService;
            _categoryService = category;
            _configuration = configuration;
            _reportService = reportService;
        }

        [Authorize]
        public async Task<IActionResult> Index(int DepartmentId = -1, int Year = -1, int StatusId = -1, string SubStatusCode = "-1", string Search = "", int PageNumber = 1, int PageLength = 10) {
            string token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            string role = _authService.GetRoleFromToken(token);
            int userId = _authService.GetAccountIdFromToken(token)??0;

            // get previous filter
            string preSearch = HttpContext.Session.GetString("topic_index_search")??"";
            int preTopicStatusSelect = HttpContext.Session.GetInt32("topic_index_topicStatusId")??-1;
            int preYearSelect = HttpContext.Session.GetInt32("topic_index_year")??DateTime.Now.Year;
            string preSubStatusSelect = HttpContext.Session.GetString("topic_index_subStatusCode")??"-1";
            int preDepartSelect = HttpContext.Session.GetInt32("topic_index_departmentId")??-1;
            int prePageLength = HttpContext.Session.GetInt32("topic_index_pageLength")??10;
            int prePageNumber = HttpContext.Session.GetInt32("topic_index_pageNumber")??1;

            // re-applied filters if its the first request
            bool isFirstRequest = DepartmentId == -1 && Year == -1 && StatusId == -1 && SubStatusCode == "-1" && Search == "" && PageNumber == 1 && PageLength == 10;
            if (isFirstRequest) {
                DepartmentId = preDepartSelect;
                Year = preYearSelect;
                StatusId = preTopicStatusSelect;
                SubStatusCode = preSubStatusSelect;
                Search = preSearch;
                PageLength = prePageLength;
            }
            PageLength = prePageLength != PageLength ? PageLength : prePageLength;

            // get select list
            var topicStatusList = await _categoryService.GetTopicStatusList();
            var subStatusList = await _categoryService.GetSubStatusList();
            var departmentList = await _categoryService.GetDepartmentListRoleBase(userId);
            var trainingPrograms = await _categoryService.GetTrainingProgramList();
            var majors = await _categoryService.GetMajorListRoleBase(userId);

            var viewModel = new TopicIndexViewPage{
                Pagination = new DataTableOptions{
                    PageNumber = PageNumber,
                    PageLength = PageLength
                },
                Year = Year,
                Status = StatusId,
                DepartmentId = DepartmentId,
                Search = Search,

                TopicStatuses = topicStatusList,
                Departments = departmentList,
                Trainingprograms = trainingPrograms,
                Majors = majors
            };

            // check to reset page number
            if (preSearch != viewModel.Search
                || preTopicStatusSelect != viewModel.Status
                || preYearSelect != viewModel.Year
                || preDepartSelect != viewModel.DepartmentId
                || prePageLength != viewModel.Pagination.PageLength) {
                viewModel.Pagination.PageNumber = 1;
            }

            var totalTopics = await _topicService.GetCountDataTable(viewModel, userId);

            if (PageLength == totalTopics) {
                viewModel.Pagination.PageNumber = 1;
            }

            var topics = await _topicService.GetDataTable(viewModel, userId);

            viewModel.Pagination.TotalRow = totalTopics;
            viewModel.DataList = topics;

            // save previous filters
            HttpContext.Session.SetString("topic_index_search", viewModel.Search);
            HttpContext.Session.SetInt32("topic_index_year", viewModel.Year);
            HttpContext.Session.SetInt32("topic_index_topicStatusId", viewModel.Status);
            HttpContext.Session.SetInt32("topic_index_departmentId", viewModel.DepartmentId);
            HttpContext.Session.SetInt32("topic_index_pageLength", viewModel.Pagination.PageLength);
            HttpContext.Session.SetInt32("topic_index_pageNumber", viewModel.Pagination.PageNumber);

            ViewBag.ReturnUrl = RouterName.QUAN_LY_DE_TAI;
            return View(viewModel);
        }

        [HttpGet("get-second-teacher")]
        [Authorize]
        public async Task<IActionResult> FindSecondTeacher(string email) {
            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");
            var data = await _topicService.GetSecondTeacherForTopicRegister(senderId ?? 0, email);
            return Ok(data);
        }

        [HttpPost("get-student")]
        [Authorize]
        public async Task<IActionResult> FindStudent(string studentCode) {
            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");
            var data = await _topicService.GetStudentTopic(senderId ?? 0, studentCode);
            return Ok(data);
        }

        [Authorize(Roles = RoleEnums.RESEARCH_ADVISOR)]
        [HttpPost("register-newtopic")]
        public async Task<IActionResult> RegisterNewTopic([FromBody] NewTopicDto newTopic) {
            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");
            var data = await _topicService.RegisterNewTopic(senderId??0, newTopic);
            return Ok(data);
        }

        [Authorize(Roles = RoleEnums.RESEARCH_ADVISOR)]
        [HttpPost("topic-delete")]
        public async Task<IActionResult> Delete(string id) {
            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");

            var hashids = new Hashids(_configuration.GetValue<string>("EncryptKey:key"), _configuration.GetValue<int>("EncryptKey:min"));
            var accualID = hashids.Decode(id).First();
            var response = await _topicService.Delete(accualID, senderId??0);
            return Ok(response);
        }

        [HttpPost("get-topic-registered-infor")]
        public async Task<IActionResult> GetRegisteredTopic(string id) {
            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");

            var hashids = new Hashids(_configuration["EncryptKey:key"], _configuration.GetValue<int>("EncryptKey:min"));
            var accualID = hashids.Decode(id).First();
            var response = await _topicService.GetTopicRegisteredDetail(accualID, senderId??0);
            return Ok(response);
        }

        [HttpGet("get-student-topic-registerd")]
        [Authorize(Roles = RoleEnums.SCIENTIFIC_RESEARCH_OFFICE + "," + RoleEnums.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM)]
        public async Task<IActionResult> GetRegisterdTopicList() {
            var data = await _reportService.GetTopicRegisterStudentList();
            if (data == null) {
                return Ok(new Response("Hệ thống lỗi, vui lòng thử lại sau"));
            } else {
                return File(data,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "topic_list.docx");
            }
        }

        [HttpGet(RouterName.GET_TOPIC_DETAIL)]
        public async Task<IActionResult> Detail(string target) {

            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");

            var data = await _topicService.GetTopicAllDetails(target, senderId??0);
            ViewBag.ReturnUrl = RouterName.QUAN_LY_DE_TAI;
            return View(data);
        }

        [HttpGet("pdf")]
        public IActionResult GetPDF() {

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "file", "KLTN_PhungDaiDong_20211374.pdf");

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf");
        }
    }

}
