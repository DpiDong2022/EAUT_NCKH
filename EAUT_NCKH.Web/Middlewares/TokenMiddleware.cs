using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Services;

namespace EAUT_NCKH.Web.Middlewares {
    public class TokenMiddleware {
        private readonly RequestDelegate _next;
        private readonly AuthService _authService;
        private readonly IConfiguration _configuration;

        public TokenMiddleware(RequestDelegate next, AuthService authService, IConfiguration configuration) {
            _next = next;
            _authService = authService;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            var token = context.Session.GetString(SessionType.USER_TOKEN);
            if (_authService.ValidateToken(token??"")) {
                context.Request.Headers.Append("Authorization", $"Bearer {token}");
            }
            else {
                token = context.Request.Cookies[CookieType.USER_TOKEN];
                if (_authService.ValidateToken(token ?? "")) {

                    var userId = _authService.GetAccountIdFromToken(token)??0;
                    var userRole = _authService.GetRoleFromToken(token);
                    token = _authService.GenerateJwtToken(userId, userRole, _configuration.GetValue<int>("Jwt:TokenExpireTimeMinute"));
                    context.Request.Headers.Append("Authorization", $"Bearer {token}");

                    context.Session.SetString(SessionType.USER_TOKEN, token);
                }
            }

            await _next(context);
        }
    }
}