using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EAUT_NCKH.Web.Extensions {
    public class LayoutFilter: IActionFilter {
        private readonly AccountService _accountService;
        private readonly AuthService _authService;

        public LayoutFilter(AccountService accountService, AuthService authService ) {
            _accountService = accountService;
            _authService = authService;
        }

        public void OnActionExecuted(ActionExecutedContext context) {}

        public void OnActionExecuting(ActionExecutingContext context) {

            try {
                var token = context.HttpContext.Session.GetString(SessionType.USER_TOKEN);
                var userId = _authService.GetAccountIdFromToken(token);
                var accountInfor = _accountService.GetAccountInformationAccount(userId??0).Result;
                var controller = context.Controller as Controller;
                controller.ViewBag.account = accountInfor.data;
                controller.ViewBag.roleId = accountInfor.data.Roleid;
                } catch {

            }
        }
    }
}
