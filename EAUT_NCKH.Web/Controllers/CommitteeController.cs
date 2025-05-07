using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Extensions;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAUT_NCKH.Web.Controllers {
    [Route(RouterName.QUAN_LY_HOI_DONG)]
    [ServiceFilter(typeof(LayoutFilter))]
    [Authorize(Roles = RoleEnums.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM + "," + RoleEnums.SCIENTIFIC_RESEARCH_OFFICE)]
    public class CommitteeController: Controller {
        private readonly CommitteeService _committeeService;
        private readonly CategoryService _categoryService;
        private readonly AuthService _authService;
        public CommitteeController(CommitteeService committeeService, CategoryService categoryService, AuthService authService) {
            _committeeService = committeeService;
            _categoryService = categoryService;
            _authService = authService;
        }

        public async Task<IActionResult> Index(CommitteeIndexViewPage options) {
            string token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            string role = _authService.GetRoleFromToken(token);
            int userId = _authService.GetAccountIdFromToken(token)??0;

            if (string.IsNullOrEmpty(options.Search)) {
                options.Search = "";
            }

            // get previous filter
            string preSearch = HttpContext.Session.GetString("committee_index_search")??"";
            int preTypeSelect = HttpContext.Session.GetInt32("committee_index_typeId")??-1;
            int preRoomSelect = HttpContext.Session.GetInt32("committee_index_roomId")??-1;
            int preBuildingSelect = HttpContext.Session.GetInt32("committee_index_buildingId")??-1;
            int preDepartSelect = HttpContext.Session.GetInt32("committee_index_departmentId")??-1;
            int prePageLength = HttpContext.Session.GetInt32("committee_index_pageLength")??10;
            int prePageNumber = HttpContext.Session.GetInt32("committee_index_pageNumber")??1;
            DateTime preFrom = DateTime.TryParse(HttpContext.Session.GetString("committee_index_from"), out var parsedFrom)
            ? parsedFrom
            : new DateTime(DateTime.Now.AddYears(-1).Year, 1, 1);

            DateTime preTo = DateTime.TryParse(HttpContext.Session.GetString("committee_index_to"), out var parsedTo)
            ? parsedTo
            : DateTime.Today.AddMonths(2);

            // get select list
            var topicStatusList = await _categoryService.GetTopicStatusList();
            var subStatusList = await _categoryService.GetSubStatusList();
            var departmentList = await _categoryService.GetDepartmentListRoleBase(userId);

            var viewModel = new CommitteeIndexViewPage{
                Pagination = new DataTableOptions{
                    PageNumber = options.Pagination.PageNumber,
                    PageLength = options.Pagination.PageLength
                },
                DepartmentId = options.DepartmentId,
                TypeId = options.TypeId,
                Search = options.Search,
                BuildingId = options.BuildingId,
                RoomId = options.RoomId,
                From = options.From,
                To = options.To,

                Departments = departmentList,
                Rooms = await _categoryService.GetAllRooms(),
                Buildings = await _categoryService.GetAllBuildings(),
                CommitteeTypes = await _categoryService.GetCommitteeTypes(),
            };

            // check to reset page number
            if (preSearch != viewModel.Search
                || preTypeSelect != viewModel.TypeId
                || preDepartSelect != viewModel.DepartmentId
                || preBuildingSelect != viewModel.BuildingId
                || preRoomSelect != viewModel.RoomId
                || prePageLength != viewModel.Pagination.PageLength) {
                viewModel.Pagination.PageNumber = 1;
            }

            var totalCommittees =  _committeeService.GetDataTableCount(userId, viewModel);

            if (options.Pagination.PageLength == totalCommittees) {
                viewModel.Pagination.PageNumber = 1;
            }

            var data = await _committeeService.GetDataTable(userId, viewModel);

            viewModel.Pagination.TotalRow = totalCommittees;
            viewModel.DataList = data.data;

            // save previous filters
            HttpContext.Session.SetString("committee_index_search", viewModel.Search);
            HttpContext.Session.SetInt32("committee_index_typeId", viewModel.TypeId);
            HttpContext.Session.SetInt32("committee_index_roomId", viewModel.RoomId);
            HttpContext.Session.SetInt32("committee_index_buildingId", viewModel.BuildingId);
            HttpContext.Session.SetInt32("committee_index_departmentId", viewModel.DepartmentId);
            HttpContext.Session.SetString("committee_index_from", viewModel.From.ToString("o")); // ISO 8601 format
            HttpContext.Session.SetString("committee_index_to", viewModel.To.ToString("o"));
            HttpContext.Session.SetInt32("committee_index_pageLength", viewModel.Pagination.PageLength);
            HttpContext.Session.SetInt32("committee_index_pageNumber", viewModel.Pagination.PageNumber);

            ViewBag.ReturnUrl = RouterName.QUAN_LY_HOI_DONG;
            return View(viewModel);
        }

        [HttpGet("get-proposal-committee")]
        public IActionResult GetProposalCommittee(string topicId) {
            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");
            var data = _committeeService.GetProposalCommittee(senderId??0, topicId);

            return Ok(data);
        }

        [HttpGet("get-final-committee")]
        public IActionResult GetFinalCommittee(string topicId) {
            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");
            var data = _committeeService.GetFinalCommittee(senderId??0, topicId);

            return Ok(data);
        }

        [HttpGet("get-defense-committee")]
        public IActionResult GetDefenseCommittee(string topicId) {
            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");
            var data = _committeeService.GetDefenseCommittee(senderId??0, topicId);

            return Ok(data);
        }
    }
}
