using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EAUT_NCKH.Web.Controllers {
    public class BaseController<T>: Controller {
        protected readonly ILogger<T> _logger;
        private readonly AuthService _authService;

        public BaseController(ILogger<T> logger, AuthService authService) {
            _logger = logger;
            _authService = authService;
        }

        protected void LogError(Exception ex) {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            _logger.LogError(ex, "Exception in {Controller}/{Action}", controller, action);
        }

        protected string GetControllerName() {
            return ControllerContext.ActionDescriptor.ControllerName;
        }

        protected string GetActionName() {
            return ControllerContext.ActionDescriptor.ActionName;
        }

        protected int GetUserId() {
            string token = HttpContext.Session.GetString(SessionType.USER_TOKEN);
            string role = _authService.GetRoleFromToken(token);
            int userId = _authService.GetAccountIdFromToken(token)??0;
            return userId;
        }
    }
}
