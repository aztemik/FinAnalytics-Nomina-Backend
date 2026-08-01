using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using FinAnalytics_Nomina.Data;
using FinAnalytics_Nomina.Models;
using FinAnalytics_Nomina.Models.DTO;
using FinAnalytics_Nomina.Security;
using FinAnalytics_Nomina.Services;

namespace FinAnalytics_Nomina.Controllers
{
    [RoutePrefix("api/periodos")]
    public class PeriodosController : ApiController
    {
        [HttpGet]
        [Route("")]
        [Autorizar(Roles = "RH,FINANZAS")]
        public IHttpActionResult ObtenerTodos()
        {
            return Ok(RespuestaApi.Ok(PeriodoDAO.ObtenerTodos()));
        }

        [HttpGet]
        [Route("{id:int}")]
        [Autorizar(Roles = "RH,FINANZAS")]
        public IHttpActionResult ObtenerPorId(int id)
        {
            var periodo = PeriodoDAO.ObtenerPorId(id);

            if (periodo == null)
            {
                return Content(HttpStatusCode.NotFound, RespuestaApi.Falla("Periodo no encontrado"));
            }

            return Ok(RespuestaApi.Ok(periodo));
        }

        [HttpPost]
        [Route("")]
        [Autorizar(Roles = "RH")]
        public IHttpActionResult Crear(PeriodoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, RespuestaApi.Falla("Datos invalidos", ErroresDe(ModelState)));
            }

            var periodo = MapearPeriodo(request);
            periodo.CreadoPor = ObtenerUsuarioId();

            var id = PeriodoDAO.Crear(periodo);

            return Content(HttpStatusCode.Created, RespuestaApi.Ok(PeriodoDAO.ObtenerPorId(id), "Periodo creado"));
        }

        [HttpPut]
        [Route("{id:int}")]
        [Autorizar(Roles = "RH")]
        public IHttpActionResult Actualizar(int id, PeriodoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, RespuestaApi.Falla("Datos invalidos", ErroresDe(ModelState)));
            }

            var actual = PeriodoDAO.ObtenerPorId(id);

            if (actual == null)
            {
                return Content(HttpStatusCode.NotFound, RespuestaApi.Falla("Periodo no encontrado"));
            }

            if (actual.Estado != "BORRADOR")
            {
                return Content(HttpStatusCode.Conflict, RespuestaApi.Falla("El periodo ya fue aprobado y no se puede modificar"));
            }

            var periodo = MapearPeriodo(request);
            periodo.Id = id;

            PeriodoDAO.Actualizar(periodo);

            return Ok(RespuestaApi.Ok(PeriodoDAO.ObtenerPorId(id), "Periodo actualizado"));
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Autorizar(Roles = "RH")]
        public IHttpActionResult Eliminar(int id)
        {
            var actual = PeriodoDAO.ObtenerPorId(id);

            if (actual == null)
            {
                return Content(HttpStatusCode.NotFound, RespuestaApi.Falla("Periodo no encontrado"));
            }

            if (actual.Estado != "BORRADOR")
            {
                return Content(HttpStatusCode.Conflict, RespuestaApi.Falla("El periodo ya fue aprobado y no se puede eliminar"));
            }

            PeriodoDAO.Eliminar(id);

            return Ok(RespuestaApi.Ok(null, "Periodo eliminado"));
        }

        [HttpPost]
        [Route("{id:int}/calcular")]
        [Autorizar(Roles = "RH")]
        public IHttpActionResult Calcular(int id)
        {
            var actual = PeriodoDAO.ObtenerPorId(id);

            if (actual == null)
            {
                return Content(HttpStatusCode.NotFound, RespuestaApi.Falla("Periodo no encontrado"));
            }

            if (actual.Estado != "BORRADOR")
            {
                return Content(HttpStatusCode.Conflict, RespuestaApi.Falla("El periodo ya fue aprobado; no se puede volver a calcular"));
            }

            ResultadoCalculoNomina resultado;

            try
            {
                resultado = CalculadoraNomina.Calcular();
            }
            catch (TipoCambioNoDisponibleException ex)
            {
                return Content(HttpStatusCode.ServiceUnavailable, RespuestaApi.Falla(ex.Message));
            }

            ReciboDAO.EliminarPorPeriodo(id);
            ReciboDAO.GuardarRecibos(id, MapearParaGuardar(resultado.Recibos));

            PeriodoDAO.ActualizarResultadoCalculo(id, resultado.TotalPercepciones, resultado.TotalDeducciones,
                resultado.TotalNeto, resultado.TotalCargaPatronal, resultado.TipoCambioUsd, resultado.FuenteTipoCambio);

            return Ok(RespuestaApi.Ok(PeriodoDAO.ObtenerPorId(id), "Nomina calculada"));
        }

        [HttpPost]
        [Route("{id:int}/aprobar")]
        [Autorizar(Roles = "FINANZAS")]
        public IHttpActionResult Aprobar(int id)
        {
            var actual = PeriodoDAO.ObtenerPorId(id);

            if (actual == null)
            {
                return Content(HttpStatusCode.NotFound, RespuestaApi.Falla("Periodo no encontrado"));
            }

            if (actual.Estado != "BORRADOR")
            {
                return Content(HttpStatusCode.Conflict, RespuestaApi.Falla("El periodo ya fue aprobado"));
            }

            PeriodoDAO.Aprobar(id, ObtenerUsuarioId());

            return Ok(RespuestaApi.Ok(PeriodoDAO.ObtenerPorId(id), "Periodo aprobado"));
        }

        private static List<(ReciboNomina Recibo, List<DetalleRecibo> Detalle)> MapearParaGuardar(List<ReciboCalculado> recibos)
        {
            return recibos.Select(r => (
                new ReciboNomina
                {
                    EmpleadoId = r.EmpleadoId,
                    SueldoBase = r.SueldoBase,
                    TotalPercepciones = r.TotalPercepciones,
                    TotalDeducciones = r.TotalDeducciones,
                    NetoPagar = r.NetoPagar,
                    CargaPatronal = r.CargaPatronal
                },
                r.Detalle.Select(d => new DetalleRecibo { Concepto = d.Concepto, Tipo = d.Tipo, Monto = d.Monto }).ToList()
            )).ToList();
        }

        private static PeriodoNomina MapearPeriodo(PeriodoRequest request)
        {
            return new PeriodoNomina
            {
                Descripcion = request.Descripcion,
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin
            };
        }

        private int ObtenerUsuarioId()
        {
            var principal = User as ClaimsPrincipal;
            return int.Parse(principal.FindFirst(JwtRegisteredClaimNames.Sub).Value);
        }

        private static List<string> ErroresDe(ModelStateDictionary modelState)
        {
            return modelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        }
    }
}
