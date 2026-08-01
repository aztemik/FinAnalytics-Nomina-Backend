using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace FinAnalytics_Nomina.Security
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class AutorizarAttribute : AuthorizationFilterAttribute
    {
        public string Roles { get; set; }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var request = actionContext.Request;
            var header = request.Headers.Authorization;

            if (header == null || header.Scheme != "Bearer" || string.IsNullOrWhiteSpace(header.Parameter))
            {
                actionContext.Response = request.CreateResponse(HttpStatusCode.Unauthorized);
                return;
            }

            var principal = JwtHelper.ValidarToken(header.Parameter);

            if (principal == null)
            {
                actionContext.Response = request.CreateResponse(HttpStatusCode.Unauthorized);
                return;
            }

            var rol = principal.FindFirst("role")?.Value;

            if (!string.IsNullOrEmpty(Roles))
            {
                var rolesPermitidos = Roles.Split(',').Select(r => r.Trim());

                if (rol == null || !rolesPermitidos.Contains(rol, StringComparer.OrdinalIgnoreCase))
                {
                    actionContext.Response = request.CreateResponse(HttpStatusCode.Forbidden);
                    return;
                }
            }

            System.Threading.Thread.CurrentPrincipal = principal;
            if (System.Web.HttpContext.Current != null)
            {
                System.Web.HttpContext.Current.User = principal;
            }
        }
    }
}
