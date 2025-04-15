using Microsoft.AspNetCore.Mvc.Filters;

namespace EAUT_NCKH.Web.Extensions {
    public class LogExceptionFilter: IExceptionFilter {
        private readonly ILogger<LogExceptionFilter> _logger;

        public LogExceptionFilter(ILogger<LogExceptionFilter> logger) {
            _logger = logger;
        }

        public void OnException(ExceptionContext context) {
            var controller = context.RouteData.Values["controller"];
            var action = context.RouteData.Values["action"];

            _logger.LogError(context.Exception, "Unhandled exception in {Controller}/{Action}", controller, action);
        }
    }
}
