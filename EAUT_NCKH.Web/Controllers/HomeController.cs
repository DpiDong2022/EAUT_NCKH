using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Extensions;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace EAUT_NCKH.Web.Controllers {
    [Route(RouterName.TRANG_CHU)]
    public class HomeController: Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly AuthService _authService;
        private readonly NotificationService _notificationService;
        public HomeController(ILogger<HomeController> logger, AuthService authService, NotificationService notificationService) {
            _logger = logger;
            _authService = authService;
            _notificationService = notificationService;
        }

        [ServiceFilter(typeof(LayoutFilter))]
        [HttpGet("")]
        public IActionResult Index() {
            string loginTOken = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            if (!_authService.ValidateToken(loginTOken??"")) {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.ReturnUrl = RouterName.TRANG_CHU;
            return View();
        }

        [HttpGet("thu-gi-khac")]
        public IActionResult Privacy() {
            return View();
        }

        [Authorize]
        [HttpGet(RouterName.GET_NOTIFIS)]
        public async Task<IActionResult> GetNotification(int startNotifiId, int length) {
            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            var senderId = _authService.GetAccountIdFromToken(token??"");
            var data = await _notificationService.GetNotification(senderId??0, startNotifiId, length);
            return Ok(data);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
