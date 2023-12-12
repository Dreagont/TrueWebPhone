using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TrueWebPhone.Controllers
{
    internal class OnlyUnauthenticatedAttribute : TypeFilterAttribute
    {
        public OnlyUnauthenticatedAttribute() : base(typeof(MyFilter))
        {

        }

        private class MyFilter : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (context.HttpContext.User.Identity.IsAuthenticated)
                {
                    context.Result = new RedirectToActionResult("Index", "Account", null);
                }
            }
        }
    }
}