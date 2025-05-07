using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Extensions;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using EAUT_NCKH.Web.DTOs.Request;

namespace EAUT_NCKH.Web.Controllers.FinalResult {
    [Route(RouterName.PHAN_CONG_PHE_DUYET_FINAL_RESULT)]
    [ServiceFilter(typeof(LayoutFilter))]
    public class FinalResultAssignmentController: Controller {

        private readonly CategoryService _categoryService;
        private readonly TopicService _topicService;
        private readonly AuthService _authService;
        private readonly FinalResultService _finalResultService;

        public FinalResultAssignmentController(CategoryService categoryService, ProposalService proposalService,
            TopicService topicService, AuthService authService, FinalResultService finalResultService) {
            _categoryService = categoryService;
            _topicService = topicService;
            _authService = authService;
            _finalResultService = finalResultService;
        }

        [Authorize(Roles = RoleEnums.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM)]
        public async Task<IActionResult> Index(int DepartmentId = -1, int Year = -1, int StatusId = -1, int PageNumber = 1, int PageLength = 10) {
            string token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            string role = _authService.GetRoleFromToken(token);
            int userId = _authService.GetAccountIdFromToken(token)??0;

            if (Year == -1) {
                Year = DateTime.Now.Year;
            }
            // get previous filter
            int preTopicStatusSelect = HttpContext.Session.GetInt32("finalresultassign_index_topicStatusId")??-1;
            int preDepartSelect = HttpContext.Session.GetInt32("finalresultassign_index_departmentId")??-1;
            int prePageLength = HttpContext.Session.GetInt32("finalresultassign_index_pageLength")??10;
            int prePageNumber = HttpContext.Session.GetInt32("finalresultassign_index_pageNumber")??1;
            int preYearSelect= HttpContext.Session.GetInt32("finalresultassign_index_year")??DateTime.Now.Year;

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

            var minStatus = (int)TopicStatusEnumId.PROPOSAL_APPROVED_BY_UNIVERSITY;
            var maxStatus = (int)TopicStatusEnumId.NOMINATED_FOR_UNIVERSITY_DEFENSE;

            topicStatusList = topicStatusList.Where(c => c.Id >= minStatus && c.Id <= maxStatus).ToList();
            var departmentList = await _categoryService.GetDepartmentListRoleBase(userId);
            var majors = await _categoryService.GetMajorListRoleBase(userId);
            var buildings = await _categoryService.GetAllBuildings();
            var rooms = await _categoryService.GetAllRooms();

            var viewModel = new FinalResultAssignmentIndexViewPage {
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

            var totalTopics = await _finalResultService.GetCountDataTableAssignment(userId, viewModel);

            if (PageLength == totalTopics) {
                viewModel.Pagination.PageNumber = 1;
            }

            var topics = await _finalResultService.GetDataTableAssignment(userId, viewModel);

            viewModel.Pagination.TotalRow = totalTopics;
            viewModel.DataList = topics;

            // save previous filters
            HttpContext.Session.SetInt32("finalresultassign_index_topicStatusId", viewModel.Status);
            HttpContext.Session.SetInt32("finalresultassign_index_departmentId", viewModel.DepartmentId);
            HttpContext.Session.SetInt32("finalresultassign_index_pageLength", viewModel.Pagination.PageLength);
            HttpContext.Session.SetInt32("finalresultassign_index_pageNumber", viewModel.Pagination.PageNumber);
            HttpContext.Session.SetInt32("finalresultassign_index_year", viewModel.Year);

            var allDepartments = await _categoryService.GetAllDepartmentAsync();
            ViewBag.ReturnUrl = RouterName.PHAN_CONG_PHE_DUYET_FINAL_RESULT;
            ViewBag.AllDepartments = allDepartments;
            return View(viewModel);
        }

        [HttpPost(RouterName.SEND_NOTIFI_SUBMIT_FINAL_RESULT)]
        [Authorize(Roles = RoleEnums.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM)]
        public async Task<IActionResult> RequireSubmitFinalResult(string target, DateTime deadline) {

            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");

            var data = _finalResultService.FinalResultRequest(senderId??0, target, deadline);

            return Ok(data);
        }

        [HttpPost("/submit-finalresult")]
        //[Authorize(Roles = RoleEnums.STUDENT)]
        public async Task<IActionResult> SubmitFinal(string topicId, IFormFile file) {

            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");

            var data = await _finalResultService.SubmitFinal(senderId??0, topicId, file);

            //var data = new Response("Loi");
            TempData["code"] = data.code;
            TempData["message"] = data.message;
            return RedirectToAction("Detail", "Topic", new { target=topicId });
        }

        [HttpGet("get-finalresult")]
        public IActionResult DownloadFinalResult(string topicId) {
            if (string.IsNullOrEmpty(topicId))
                return BadRequest("Invalid topic ID.");

            var filePath = $"D:/UploadedFiles/FinalResult/{topicId}.zip";

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = "application/zip";
            var fileName = $"{topicId}.zip";

            return File(fileBytes, contentType, fileName);
        }

        [HttpPost("modify-committee-topic")]
        [Authorize(Roles = RoleEnums.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM)]
        public async Task<IActionResult> ModifyCommittee(CommitteeAddRequest request) {

            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");

            var data = await _finalResultService.AddOrEditCommitteeAssignment(senderId??0, request);
            return Ok(data);
        }

        [HttpPost("nominate")]
        public async Task<IActionResult> Nominate(string topicId, int userId) {
            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");

            var data = await _finalResultService.NominateTopic(senderId??0, topicId);
            TempData["code"] = data.code;
            TempData["message"] = data.message;
            return RedirectToAction("Index");
        }
    }
}
