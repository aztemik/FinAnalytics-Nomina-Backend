using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using FinAnalytics_Nomina.Data;
using FinAnalytics_Nomina.Models.DTO;
using FinAnalytics_Nomina.Security;

namespace FinAnalytics_Nomina.Controllers
{
    [RoutePrefix("api/parametros")]
    public class ParametrosController : ApiController
    {
        [HttpGet]
        [Route("")]
        [Autorizar(Roles = "ADMIN,RH,FINANZAS")]
        public IHttpActionResult ObtenerTodos()
        {
            return Ok(RespuestaApi.Ok(ParametroDAO.ObtenerTodos()));
        }

        [HttpPut]
        [Route("{id:int}")]
        [Autorizar(Roles = "ADMIN")]
        public IHttpActionResult Actualizar(int id, ParametroActualizarRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, RespuestaApi.Falla("Datos invalidos", ErroresDe(ModelState)));
            }

            if (ParametroDAO.ObtenerPorId(id) == null)
            {
                return Content(HttpStatusCode.NotFound, RespuestaApi.Falla("Parametro no encontrado"));
            }

            ParametroDAO.ActualizarValor(id, request.Valor);

            return Ok(RespuestaApi.Ok(ParametroDAO.ObtenerPorId(id), "Parametro actualizado"));
        }

        private static List<string> ErroresDe(ModelStateDictionary modelState)
        {
            return modelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        }
    }
}
