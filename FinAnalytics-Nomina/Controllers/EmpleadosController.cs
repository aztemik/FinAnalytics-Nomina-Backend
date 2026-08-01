using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using FinAnalytics_Nomina.Data;
using FinAnalytics_Nomina.Models;
using FinAnalytics_Nomina.Models.DTO;
using FinAnalytics_Nomina.Security;

namespace FinAnalytics_Nomina.Controllers
{
    [RoutePrefix("api/empleados")]
    public class EmpleadosController : ApiController
    {
        [HttpGet]
        [Route("")]
        [Autorizar(Roles = "RH,FINANZAS")]
        public IHttpActionResult ObtenerTodos()
        {
            return Ok(RespuestaApi.Ok(EmpleadoDAO.ObtenerTodos()));
        }

        [HttpGet]
        [Route("{id:int}")]
        [Autorizar(Roles = "RH,FINANZAS")]
        public IHttpActionResult ObtenerPorId(int id)
        {
            var empleado = EmpleadoDAO.ObtenerPorId(id);

            if (empleado == null)
            {
                return Content(HttpStatusCode.NotFound, RespuestaApi.Falla("Empleado no encontrado"));
            }

            return Ok(RespuestaApi.Ok(empleado));
        }

        [HttpPost]
        [Route("")]
        [Autorizar(Roles = "RH")]
        public IHttpActionResult Crear(EmpleadoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, RespuestaApi.Falla("Datos invalidos", ErroresDe(ModelState)));
            }

            var errores = ValidarUnicidad(request, null);

            if (errores.Count > 0)
            {
                return Content(HttpStatusCode.BadRequest, RespuestaApi.Falla("Datos invalidos", errores));
            }

            var empleado = MapearEmpleado(request);
            empleado.Activo = true;

            var id = EmpleadoDAO.Crear(empleado);

            return Content(HttpStatusCode.Created, RespuestaApi.Ok(EmpleadoDAO.ObtenerPorId(id), "Empleado creado"));
        }

        [HttpPut]
        [Route("{id:int}")]
        [Autorizar(Roles = "RH")]
        public IHttpActionResult Actualizar(int id, EmpleadoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, RespuestaApi.Falla("Datos invalidos", ErroresDe(ModelState)));
            }

            if (EmpleadoDAO.ObtenerPorId(id) == null)
            {
                return Content(HttpStatusCode.NotFound, RespuestaApi.Falla("Empleado no encontrado"));
            }

            var errores = ValidarUnicidad(request, id);

            if (errores.Count > 0)
            {
                return Content(HttpStatusCode.BadRequest, RespuestaApi.Falla("Datos invalidos", errores));
            }

            var empleado = MapearEmpleado(request);
            empleado.Id = id;

            EmpleadoDAO.Actualizar(empleado);

            return Ok(RespuestaApi.Ok(EmpleadoDAO.ObtenerPorId(id), "Empleado actualizado"));
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Autorizar(Roles = "RH")]
        public IHttpActionResult Eliminar(int id)
        {
            if (EmpleadoDAO.ObtenerPorId(id) == null)
            {
                return Content(HttpStatusCode.NotFound, RespuestaApi.Falla("Empleado no encontrado"));
            }

            EmpleadoDAO.Desactivar(id);

            return Ok(RespuestaApi.Ok(null, "Empleado desactivado"));
        }

        private static List<string> ValidarUnicidad(EmpleadoRequest request, int? idExcluir)
        {
            var errores = new List<string>();

            if (EmpleadoDAO.ExisteNumeroEmpleado(request.NumeroEmpleado, idExcluir))
            {
                errores.Add("El numero de empleado ya existe");
            }

            if (EmpleadoDAO.ExisteRfc(request.Rfc, idExcluir))
            {
                errores.Add("El RFC ya esta registrado");
            }

            return errores;
        }

        private static Empleado MapearEmpleado(EmpleadoRequest request)
        {
            return new Empleado
            {
                NumeroEmpleado = request.NumeroEmpleado,
                Nombre = request.Nombre,
                Apellidos = request.Apellidos,
                Rfc = request.Rfc,
                Puesto = request.Puesto,
                Departamento = request.Departamento,
                TipoContratacion = request.TipoContratacion,
                Moneda = request.Moneda,
                SalarioMensual = request.SalarioMensual,
                FechaIngreso = request.FechaIngreso,
                UsuarioId = request.UsuarioId
            };
        }

        private static List<string> ErroresDe(ModelStateDictionary modelState)
        {
            return modelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        }
    }
}
