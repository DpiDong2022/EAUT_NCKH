using Microsoft.AspNetCore.Mvc;

namespace EAUT_NCKH.Web.Services {
    public class LogService<T> {
        private readonly ILogger<T> _logger;
        public LogService(ILogger<T> logger) {
            _logger = logger;
        }

        public void LogError(Exception ex) {

            _logger.LogError(ex, "Exception occurred. Message: {Message} ---- Inner Message: {InnerMessage}",
                ex.Message,
                ex.InnerException?.Message ?? "None");
        }
    }
}
