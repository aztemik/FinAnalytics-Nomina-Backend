using System.Net;
using System.Web.Http;
using FinAnalytics_Nomina.Models.DTO;
using FinAnalytics_Nomina.Security;
using FinAnalytics_Nomina.Services;

namespace FinAnalytics_Nomina.Controllers
{
    [RoutePrefix("api/tipocambio")]
    public class TipoCambioController : ApiController
    {
        [HttpGet]
        [Route("")]
        [Autorizar(Roles = "RH,FINANZAS")]
        public IHttpActionResult Obtener()
        {
            var resultado = TipoCambioService.Obtener();

            if (resultado == null)
            {
                return Content(HttpStatusCode.ServiceUnavailable, RespuestaApi.Falla(
                    "No se pudo obtener el tipo de cambio: el servicio externo no respondio y no hay ningun valor en cache. Se requiere captura manual."));
            }

            return Ok(RespuestaApi.Ok(resultado));
        }
    }
}
