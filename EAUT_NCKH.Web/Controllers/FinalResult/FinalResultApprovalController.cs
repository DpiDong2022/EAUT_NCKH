using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Extensions;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace EAUT_NCKH.Web.Controllers.FinalResult {
    [Route(RouterName.PHE_DUYET_FINAL_RESULT)]
    [ServiceFilter(typeof(LayoutFilter))]
    public class FinalResultApprovalController: Controller {
        private readonly CategoryService _categoryService;
        private readonly TopicService _topicService;
        private readonly AuthService _authService;
        private readonly FinalResultService _finalResultService;

        public FinalResultApprovalController(CategoryService categoryService, ProposalService proposalService,
            TopicService topicService, AuthService authService, FinalResultService finalResultService) {
            _categoryService = categoryService;
            _topicService = topicService;
            _authService = authService;
            _finalResultService = finalResultService;
        }
        [Authorize]
        public async Task<IActionResult> Index(FinalResultApprovalIndexViewPage options) {
            string token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            string role = _authService.GetRoleFromToken(token);
            int userId = _authService.GetAccountIdFromToken(token)??0;

            // get previous filter
            int preTopicStatusSelect = HttpContext.Session.GetInt32("finalapprovel_index_topicStatusId")??-1;
            int preDepartSelect = HttpContext.Session.GetInt32("finalapprovel_index_departmentId")??-1;
            int prePageLength = HttpContext.Session.GetInt32("finalapprovel_index_pageLength")??10;
            int prePageNumber = HttpContext.Session.GetInt32("finalapprovel_index_pageNumber")??1;

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
            topicStatusList = topicStatusList.Where(c => c.Id >= (int)TopicStatusEnumId.FINAL_REVIEW_ASSIGNMENT && c.Id <= (int)TopicStatusEnumId.FINAL_APPROVED_BY_FACULTY).ToList();
            var departmentList = await _categoryService.GetAllDepartmentAsync();
            var substatuses = await _categoryService.GetListSubstatuses();
            var criterias = await _categoryService.GetListOfCriteria();
            //var majors = await _categoryService.GetMajorListRoleBase(userId);
            //var buildings = await _categoryService.GetAllBuildings();
            //var rooms = await _categoryService.GetAllRooms();

            var viewModel = new FinalResultApprovalIndexViewPage{
                Pagination = new DataTableOptions{
                    PageNumber = options.Pagination.PageNumber,
                    PageLength = options.Pagination.PageLength
                },
                StatusId = options.StatusId,
                DepartmentId = options.DepartmentId,

                TopicStatuses = topicStatusList,
                Departments = departmentList,
                Substatuses = substatuses,
                Criteriaevaluations = criterias
            };

            // check to reset page number
            if (preTopicStatusSelect != viewModel.StatusId
                || preDepartSelect != viewModel.DepartmentId
                || prePageLength != viewModel.Pagination.PageLength) {
                viewModel.Pagination.PageNumber = 1;
            }

            var totalTopics = await _finalResultService.GetCountDataTableApproval(userId, viewModel);

            if (options.Pagination.PageLength == totalTopics) {
                viewModel.Pagination.PageNumber = 1;
            }

            var topics = await _finalResultService.GetDataTableApproval(userId, viewModel);
            viewModel.Pagination.TotalRow = totalTopics;
            viewModel.DataList = topics;

            // save previous filters
            HttpContext.Session.SetInt32("finalapprovel_index_topicStatusId", viewModel.StatusId);
            HttpContext.Session.SetInt32("finalapprovel_index_departmentId", viewModel.DepartmentId);
            HttpContext.Session.SetInt32("finalapprovel_index_pageLength", viewModel.Pagination.PageLength);
            HttpContext.Session.SetInt32("finalapprovel_index_pageNumber", viewModel.Pagination.PageNumber);

            var allDepartments = await _categoryService.GetAllDepartmentAsync();
            ViewBag.ReturnUrl = RouterName.PHE_DUYET_FINAL_RESULT;
            ViewBag.AllDepartments = allDepartments;
            return View(viewModel);
        }

        [HttpPost("final-evaluation")]
        public async Task<IActionResult> FinalResultEvaluate(FinalResultEvaluation evaluations) {
            string token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            string role = _authService.GetRoleFromToken(token);
            int userId = _authService.GetAccountIdFromToken(token)??0;
            var data = await _finalResultService.EvaluateFinalResult(evaluations, userId);

            //var data = new Response("Loi");
            TempData["code"] = data.code;
            TempData["message"] = data.message;
            return RedirectToAction("Index", "FinalResultApproval");
        }

        [HttpGet("get-evaluations")]
        public async Task<IActionResult> GetEvaluationListMember(string topicId) {
            string token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            string role = _authService.GetRoleFromToken(token);
            int userId = _authService.GetAccountIdFromToken(token)??0;
            var data = await _finalResultService.GetEvaluationList(topicId, userId);

            return Ok(data);
        }

        [HttpGet("get-evaluations2")]
        public async Task<IActionResult> GetEvaluationListMember2(string topicId, int userId) {;
            var data = await _finalResultService.GetEvaluationList(topicId, userId);

            return Ok(data);
        }
    }
}
