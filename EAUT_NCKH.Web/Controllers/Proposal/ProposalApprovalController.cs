using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EAUT_NCKH.Web.Extensions;
using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.Controllers {
    [Route(RouterName.PHE_DUYET_THUYET_MINH)]
    [Authorize]
    [ServiceFilter(typeof(LayoutFilter))]
    public class ProposalApprovalController: Controller {
        private readonly CategoryService _categoryService;
        private readonly ProposalService _proposalService;
        private readonly AuthService _authService;

        public ProposalApprovalController(CategoryService categoryService, ProposalService proposal, AuthService authService){
            _categoryService = categoryService;
            _proposalService =  proposal;
            _authService = authService;
        }

        [Authorize]
        public async Task<IActionResult> Index(ProposalAssignedIndexViewPage options) {
            string token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            string role = _authService.GetRoleFromToken(token);
            int userId = _authService.GetAccountIdFromToken(token)??0;

            // get previous filter
            int preTopicStatusSelect = HttpContext.Session.GetInt32("proposalapprovel_index_topicStatusId")??-1;
            int preDepartSelect = HttpContext.Session.GetInt32("proposalapprovel_index_departmentId")??-1;
            int prePageLength = HttpContext.Session.GetInt32("proposalapprovel_index_pageLength")??10;
            int prePageNumber = HttpContext.Session.GetInt32("proposalapprovel_index_pageNumber")??1;

            // re-applied filters if its the first request
            bool isFirstRequest = options.DepartmentId == -1 && options.StatusId == -1 && options.Pagination.PageNumber == 1 && options.Pagination.PageLength == 10;
            if (isFirstRequest) {
                options.DepartmentId = preDepartSelect;
                options.StatusId = preTopicStatusSelect;
                options.Pagination.PageLength = prePageLength;
            }
            options.Pagination.PageLength = prePageLength != options.Pagination.PageLength ? options.Pagination.PageLength : prePageLength;

            // get select list
            var topicStatusList = await _categoryService.GetTopicStatusList();
            topicStatusList = topicStatusList.Where(c => c.Id >= (int)TopicStatusEnumId.PROPOSAL_REVIEW_ASSIGNMENT && c.Id <= (int)TopicStatusEnumId.PROPOSAL_APPROVED_BY_UNIVERSITY).ToList();
            var departmentList = await _categoryService.GetAllDepartmentAsync();
            var substatuses = await _categoryService.GetListSubstatuses();
            //var majors = await _categoryService.GetMajorListRoleBase(userId);
            //var buildings = await _categoryService.GetAllBuildings();
            //var rooms = await _categoryService.GetAllRooms();

            var viewModel = new ProposalAssignedIndexViewPage{
                Pagination = new DataTableOptions{
                    PageNumber = options.Pagination.PageNumber,
                    PageLength = options.Pagination.PageLength
                },
                StatusId = options.StatusId,
                DepartmentId = options.DepartmentId,

                TopicStatuses = topicStatusList,
                Departments = departmentList,
                Substatuses = substatuses
            };

            // check to reset page number
            if (preTopicStatusSelect != viewModel.StatusId
                || preDepartSelect != viewModel.DepartmentId
                || prePageLength != viewModel.Pagination.PageLength) {
                viewModel.Pagination.PageNumber = 1;
            }

            var totalTopics = _proposalService.GetTopicProposalAssignedListCount(userId, viewModel);

            if (options.Pagination.PageLength == totalTopics) {
                viewModel.Pagination.PageNumber = 1;
            }

            var topics = _proposalService.GetTopicProposalAssignedList(userId, viewModel);
            viewModel.Pagination.TotalRow = totalTopics;
            viewModel.DataList = topics.data;

            // save previous filters
            HttpContext.Session.SetInt32("proposalapprovel_index_topicStatusId", viewModel.StatusId);
            HttpContext.Session.SetInt32("proposalapprovel_index_departmentId", viewModel.DepartmentId);
            HttpContext.Session.SetInt32("proposalapprovel_index_pageLength", viewModel.Pagination.PageLength);
            HttpContext.Session.SetInt32("proposalapprovel_index_pageNumber", viewModel.Pagination.PageNumber);

            var allDepartments = await _categoryService.GetAllDepartmentAsync();
            ViewBag.ReturnUrl = RouterName.PHE_DUYET_THUYET_MINH;
            ViewBag.AllDepartments = allDepartments;
            return View(viewModel);
        }

        [HttpPost("approve-proposal")]
        [AutoValidateAntiforgeryToken]
        public IActionResult ApproveProposal(int statusId, string feedback, string topicId) {

            var userId = GetUserId();
            var data = _proposalService.ApproveProposal(userId, topicId, statusId, feedback);

            TempData["code"] = data.code;
            TempData["message"] = data.message;
            //return Ok(data);
            return RedirectToAction("Index", "ProposalApproval");
        }

        [HttpPost("cancel-proposal-approval")]
        [AutoValidateAntiforgeryToken]
        public IActionResult CancelProposalApproval(string topicId) {

            var userId = GetUserId();
            var data = _proposalService.CancelProposalApproval(userId, topicId);

            TempData["code"] = data.code;
            TempData["message"] = data.message;
            //return Ok(data);
            return RedirectToAction("Index", "ProposalApproval");
        }

        private int GetUserId() {
            string token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            string role = _authService.GetRoleFromToken(token);
            int userId = _authService.GetAccountIdFromToken(token)??0;
            return userId;
        }
    }
}
