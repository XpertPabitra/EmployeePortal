using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EmployeePortal.Controllers
{
    /// <summary>
    /// This controller serves as a security layer. 
    /// Any controller inheriting from this will require a valid Admin Session.
    /// </summary>
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 1. Retrieve the Session key we set during Login
            var adminSession = HttpContext.Session.GetString("AdminName");

            // 2. If the session is null or empty, the user is not authorized
            if (string.IsNullOrEmpty(adminSession))
            {
                // 3. Halt the current request and force a redirect to the Login page
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }

            // 4. Continue with the requested action if authorized
            base.OnActionExecuting(context);
        }
    }
}