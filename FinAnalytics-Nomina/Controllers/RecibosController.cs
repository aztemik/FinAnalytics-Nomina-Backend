using System.Net;
using System.Security.Claims;
using System.Web.Http;
using FinAnalytics_Nomina.Data;
using FinAnalytics_Nomina.Models.DTO;
using FinAnalytics_Nomina.Security;

namespace FinAnalytics_Nomina.Controllers
{
    [RoutePrefix("api/recibos")]
    public class RecibosController : ApiController
    {
        [HttpGet]
        [Route("")]
        [Autorizar(Roles = "RH,FINANZAS")]
        public IHttpActionResult ObtenerPorPeriodo(int periodoId)
        {
            return Ok(RespuestaApi.Ok(ReciboDAO.ObtenerPorPeriodo(periodoId)));
        }

        [HttpGet]
        [Route("mis-recibos")]
        [Autorizar(Roles = "EMPLEADO")]
        public IHttpActionResult MisRecibos()
        {
            // El empleado no manda ningun id: se extrae del claim firmado, nunca de la peticion.
            // AuthController ya deberia impedir que un EMPLEADO sin vinculo reciba un token sin
            // este claim, pero un token viejo emitido antes de ese arreglo podria no tenerlo:
            // se maneja con un error claro en vez de tronar con NullReferenceException.
            var empleadoIdClaim = ((ClaimsPrincipal)User).FindFirst("empleadoId")?.Value;

            if (empleadoIdClaim == null)
            {
                return Content(HttpStatusCode.Forbidden, RespuestaApi.Falla(
                    "Tu cuenta de portal aun no esta vinculada a un empleado. Vuelve a iniciar sesion o contacta a Recursos Humanos."));
            }

            return Ok(RespuestaApi.Ok(ReciboDAO.ObtenerPorEmpleado(int.Parse(empleadoIdClaim))));
        }

        [HttpGet]
        [Route("{id:int}")]
        [Autorizar(Roles = "RH,FINANZAS,EMPLEADO")]
        public IHttpActionResult ObtenerPorId(int id)
        {
            var recibo = ReciboDAO.ObtenerPorId(id);

            if (recibo == null)
            {
                return Content(HttpStatusCode.NotFound, RespuestaApi.Falla("Recibo no encontrado"));
            }

            // Filtro a nivel de dato: RH y FINANZAS ven cualquier recibo, pero un EMPLEADO
            // solo el propio. El rol ya paso el filtro de AutorizarAttribute; esto compara
            // el dueño real del recibo contra el claim del token.
            var principal = (ClaimsPrincipal)User;
            var rol = principal.FindFirst("role")?.Value;

            if (rol == "EMPLEADO")
            {
                var empleadoIdClaim = principal.FindFirst("empleadoId")?.Value;

                if (empleadoIdClaim == null || int.Parse(empleadoIdClaim) != recibo.EmpleadoId)
                {
                    return Content(HttpStatusCode.Forbidden, RespuestaApi.Falla("No tienes permiso para ver este recibo"));
                }
            }

            return Ok(RespuestaApi.Ok(recibo));
        }
    }
}
