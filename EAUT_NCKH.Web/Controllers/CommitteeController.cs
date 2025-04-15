using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EAUT_NCKH.Web.Controllers {
    public class CommitteeController: Controller {
        private readonly CommitteeService _committeeService;
        private readonly CategoryService _categoryService;
        public CommitteeController(CommitteeService committeeService, CategoryService categoryService) {
            _committeeService = committeeService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index() {
            ViewBag.CommitteeTypes = await _committeeService.ViewCommitteeTypes();
            ViewBag.TrainingProgramList = _categoryService.GetTrainingProgramList().Result.OrderByDescending(c => c.Displayorder).ToList();
            ViewBag.majorList = await _categoryService.GetMajorList();
            return View();
        }
    }
}
