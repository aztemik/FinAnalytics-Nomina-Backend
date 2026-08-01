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
            int empleadoId = int.Parse(((ClaimsPrincipal)User).FindFirst("empleadoId").Value);

            return Ok(RespuestaApi.Ok(ReciboDAO.ObtenerPorEmpleado(empleadoId)));
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
