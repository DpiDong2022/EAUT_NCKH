using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAUT_NCKH.Web.Controllers {
    [Authorize]
    [Route("")]
    public class AuthController: Controller {
        private readonly AccountService _accountService;
        public AuthController(AccountService accountService) {
            _accountService = accountService;
        }

        [HttpPost]
        [Route("oracle")]
        public async Task<IActionResult> ChangePassword(string currentPass, string newPass, string confirmPass) {
            var userId = HttpContext.Session.GetString(SessionType.USER_ID);
            var response = new Response();
            if (string.IsNullOrEmpty(userId)) {
                response.code = 1;
                response.message = "Phiên đăng nhập của bạn đã hết bạn, vui lòng đăng nhập lại!";
                return Ok(response);
            }

            return Ok(await _accountService.ChangePasswordAsync(int.Parse(userId), currentPass, newPass, confirmPass));
        }
    }
}
