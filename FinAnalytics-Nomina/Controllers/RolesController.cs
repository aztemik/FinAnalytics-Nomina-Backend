using System.Web.Http;
using FinAnalytics_Nomina.Data;
using FinAnalytics_Nomina.Models.DTO;
using FinAnalytics_Nomina.Security;

namespace FinAnalytics_Nomina.Controllers
{
    [RoutePrefix("api/roles")]
    [Autorizar(Roles = "ADMIN")]
    public class RolesController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult ObtenerTodos()
        {
            return Ok(RespuestaApi.Ok(RolDAO.ObtenerTodos()));
        }
    }
}
