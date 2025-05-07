using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EAUT_NCKH.Web.Extensions;
using Microsoft.Extensions.FileProviders;
using EAUT_NCKH.Web.DTOs.Request;
using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.Controllers {
    [Route(RouterName.PHAN_CONG_PHE_DUYET_TM)]
    [ServiceFilter(typeof(LayoutFilter))]
    public class ProposalAssignmentController: Controller {
        private readonly CategoryService _categoryService;
        private readonly TopicService _topicService;
        private readonly ProposalService _proposalService;
        private readonly AuthService _authService;

        public ProposalAssignmentController(CategoryService categoryService, ProposalService proposalService,
            TopicService topicService, AuthService authService) {
            _categoryService = categoryService;
            _topicService = topicService;
            _authService = authService;
            _proposalService = proposalService;
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
            int preTopicStatusSelect = HttpContext.Session.GetInt32("proposalassign_index_topicStatusId")??-1;
            int preDepartSelect = HttpContext.Session.GetInt32("proposalassign_index_departmentId")??-1;
            int prePageLength = HttpContext.Session.GetInt32("proposalassign_index_pageLength")??10;
            int prePageNumber = HttpContext.Session.GetInt32("proposalassign_index_pageNumber")??1;
            int preYearSelect= HttpContext.Session.GetInt32("proposalassign_index_year")??DateTime.Now.Year;

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
            topicStatusList = topicStatusList.Where(c => c.Id >= 2 && c.Id  <= 8).ToList();
            var departmentList = await _categoryService.GetDepartmentListRoleBase(userId);
            var majors = await _categoryService.GetMajorListRoleBase(userId);
            var buildings = await _categoryService.GetAllBuildings();
            var rooms = await _categoryService.GetAllRooms();

            var viewModel = new TopicIndexViewPage{
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

            var totalTopics = await _topicService.GetCountDataTable(viewModel, userId);

            if (PageLength == totalTopics) {
                viewModel.Pagination.PageNumber = 1;
            }

            var topics = await _topicService.GetDataTable(viewModel, userId);

            viewModel.Pagination.TotalRow = totalTopics;
            viewModel.DataList = topics;

            // save previous filters
            HttpContext.Session.SetInt32("proposalassign_index_topicStatusId", viewModel.Status);
            HttpContext.Session.SetInt32("proposalassign_index_departmentId", viewModel.DepartmentId);
            HttpContext.Session.SetInt32("proposalassign_index_pageLength", viewModel.Pagination.PageLength);
            HttpContext.Session.SetInt32("proposalassign_index_pageNumber", viewModel.Pagination.PageNumber);
            HttpContext.Session.SetInt32("proposalassign_index_year", viewModel.Year);

            var allDepartments = await _categoryService.GetAllDepartmentAsync();
            ViewBag.ReturnUrl = RouterName.PHAN_CONG_PHE_DUYET_TM;
            ViewBag.AllDepartments = allDepartments;
            return View(viewModel);
        }

        [HttpPost(RouterName.SEND_NOTIFI_SUBMIT_PROPOSAL)]
        [Authorize(Roles = RoleEnums.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM)]
        public async Task<IActionResult> RequireSubmitProposal(string target, DateTime deadline) {

            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");

            var data = await _proposalService.RequireToSubmitProposal(target, deadline, senderId??0);

            return Ok(data);
        }

        [HttpPost("/submit-proposal")]
        //[Authorize(Roles = RoleEnums.STUDENT)]
        public async Task<IActionResult> SubmitProposal(string topicId, IFormFile file, string note) {

            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");

            var data = await _proposalService.SubmitProposal(senderId??0, topicId, file, note);

            return Ok(data);
        }

        [HttpGet("view-proposal-file")]
        public IActionResult ViewProposal(string target) {

           var res = _proposalService.GetProposalFile(target);
            return File(res.data, "application/pdf");
        }

        //[HttpPost("modify-committee")]
        //public IActionResult ModifyCommittee(CommitteeAddRequest request) {

        //    return Ok();
        //}

        [HttpPost("modify-committee")]
        [Authorize(Roles = RoleEnums.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM)]
        public IActionResult ModifyCommittee(CommitteeAddRequest request) {

            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");

            var data = _proposalService.AddOrEditCommitteeAssignment(senderId??0, request);
            return Ok(data);
        }

    }
}
