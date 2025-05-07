using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EAUT_NCKH.Web.Controllers {
    [Route("")]
    [Route(RouterName.LOGIN)]
    public class LoginController: Controller {
        private readonly AuthService _authService;
        private readonly AccountService _accountService;
        private readonly IConfiguration _configuration;
        private readonly EntityDataContext  _dbContext;

        public LoginController(AuthService authService, AccountService accountService, IConfiguration configuration, EntityDataContext entityDataContext) {
            _authService = authService;
            _accountService = accountService;
            _configuration = configuration;
            _dbContext = entityDataContext;
        }

        [HttpGet("dang-nhap")]
        public IActionResult Index() {
            var token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            if (_authService.ValidateToken(token)) {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel() { AccountName= "nguyenthivan01@gmail.com", Password="12345678" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("login")]
        [HttpPost("dang-nhap")]
        public async Task<IActionResult> Index(LoginViewModel request) {

            if (!ModelState.IsValid) {
                return View(request);
            }

            //string rawSql = $"SELECT * FROM account WHERE email = '{request.AccountName}' AND password = '{request.Password}'";

            //var accounts = await _dbContext.Accounts
            //                .FromSqlRaw(rawSql).Include(c => c.Role)
            //                .ToListAsync();

            //if (accounts == null || accounts.Count == 0) {
            //    ModelState.AddModelError("AccountName", "Tài khoản hoặc mật khẩu không chính xác");
            //    return View(request);
            //}

            var accounts = await _accountService.GetAll(new AccountRequestOptions(request.AccountName));
            if (accounts == null || accounts.Count() == 0) {

                ModelState.AddModelError("AccountName", "Tài khoản hoặc mật khẩu không chính xác");
                return View(request);
            }

            var account = accounts?.First();

            //if (BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) {
            if (request.Password != account.Password) {

                ModelState.AddModelError("AccountName", "Tài khoản hoặc mật khẩu không chính xác");
                return View(request);
            }


            var accountt = accounts.First();
            int tokenExpireMinutes = _configuration.GetValue<int>("Jwt:TokenExpireTimeMinute");
            var token = _authService.GenerateJwtToken(accountt.Id, accountt.Role.Name, tokenExpireMinutes);

            // save main token in session, reset token in cookies
            HttpContext.Session.SetString(SessionType.USER_TOKEN, token);

            if (request.IsSaveAccount) {
                // Tạo thêm jwt thứ 2 là jwt reset lưu vào cookies và để thời hạn của nó là 1 tháng, mã jwt chính sẽ để là 10 phút, như vậy
                // mỗi khi mỗi 10 phút jwt hết hạn, thì ta sẽ phát hiện và kiểm tra nếu jwt thứ reset tồn tại và còn hiệu lực ta sẽ tạo jwt chính
                // nhờ đó đăng nhập sẽ được lưu liên tục trong 1 tháng mà ko cần đăng nhập lại

                int refreshTokenExpireMinutes = _configuration.GetValue<int>("Jwt:RefreshTokenExpireTimeMinute");
                var refreshToken = _authService.GenerateJwtToken(accounts.First().Id, accounts.First().Role.Name, refreshTokenExpireMinutes);


                var cookieOptions = new CookieOptions {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.Now.AddMinutes(refreshTokenExpireMinutes)
                };

                HttpContext.Response.Cookies.Append(CookieType.USER_TOKEN, refreshToken, cookieOptions);
            }

            //var claims = new List<Claim>
            //{
            //    new Claim(ClaimTypes.NameIdentifier, account.First().Id.ToString()),
            //    new Claim(ClaimTypes.Role, account.First().Role.Name)
            //};

            //var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            //var principal = new ClaimsPrincipal(identity);

            //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("dang-xuat")]
        public IActionResult Logout() {

            HttpContext.Session.Clear();
            foreach (var cookie in Request.Cookies.Keys) {
                Response.Cookies.Delete(cookie);
            }
            return RedirectToAction("Index", "Login");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> RequestOTP(string email) {

            var response = await _accountService.GetOTP(email);
            return Ok(response);
        }

        [HttpPost("request-password")]
        public async Task<IActionResult> RequestPassword(string otp, string email) {

            var response = await _accountService.VarifyOTP(otp, email);
            return Ok(response);
        }
    }
}
