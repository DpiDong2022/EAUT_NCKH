using Microsoft.AspNetCore.Mvc;

namespace EAUT_NCKH.Web.Controllers {
    public class BaseController<T>: Controller {
        protected readonly ILogger<T> _logger;

        public BaseController(ILogger<T> logger) {
            _logger = logger;
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
    }
}
