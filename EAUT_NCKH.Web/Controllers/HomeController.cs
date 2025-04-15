using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace EAUT_NCKH.Web.Controllers {
    [Route("trang-chu")]
    public class HomeController: Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly AuthService _authService;

        public HomeController(ILogger<HomeController> logger, AuthService authService) {
            _logger = logger;
            _authService = authService;
        }

        [HttpGet("")]
        public IActionResult Index() {
            string loginTOken = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            if (!_authService.ValidateToken(loginTOken??"")) {
                return RedirectToAction("Index", "Login");
            }

            var role = User.IsInRole(RoleEnums.STUDENT);
            var id = User.FindFirst(c => c.ValueType == ClaimTypes.NameIdentifier);
            ViewBag.ReturnUrl = RouterName.TRANG_CHU;
            return View();
        }

        [HttpGet("thu-gi-khac")]
        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
