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
    [RoutePrefix("api/usuarios")]
    public class UsuariosController : ApiController
    {
        // RH tambien puede listar usuarios: los necesita para vincular un empleado
        // con su cuenta de portal (rol EMPLEADO) en empleados.html. El resto de
        // operaciones sobre usuarios (alta, edicion, baja, y el detalle por id)
        // siguen exclusivas de ADMIN.
        [HttpGet]
        [Route("")]
        [Autorizar(Roles = "ADMIN,RH")]
        public IHttpActionResult ObtenerTodos()
        {
            return Ok(RespuestaApi.Ok(UsuarioDAO.ObtenerTodos()));
        }

        [HttpGet]
        [Route("{id:int}")]
        [Autorizar(Roles = "ADMIN")]
        public IHttpActionResult ObtenerPorId(int id)
        {
            var usuario = UsuarioDAO.ObtenerPorId(id);

            if (usuario == null)
            {
                return Content(HttpStatusCode.NotFound, RespuestaApi.Falla("Usuario no encontrado"));
            }

            return Ok(RespuestaApi.Ok(usuario));
        }

        [HttpPost]
        [Route("")]
        [Autorizar(Roles = "ADMIN")]
        public IHttpActionResult Crear(UsuarioCrearRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, RespuestaApi.Falla("Datos invalidos", ErroresDe(ModelState)));
            }

            if (UsuarioDAO.ExisteUsername(request.Username))
            {
                return Content(HttpStatusCode.BadRequest,
                    RespuestaApi.Falla("Datos invalidos", new List<string> { "El nombre de usuario ya existe" }));
            }

            var usuario = new Usuario
            {
                Username = request.Username,
                PasswordHash = PasswordHelper.Hash(request.Password),
                NombreCompleto = request.NombreCompleto,
                RolId = request.RolId,
                Activo = true
            };

            var id = UsuarioDAO.Crear(usuario);

            return Content(HttpStatusCode.Created, RespuestaApi.Ok(UsuarioDAO.ObtenerPorId(id), "Usuario creado"));
        }

        [HttpPut]
        [Route("{id:int}")]
        [Autorizar(Roles = "ADMIN")]
        public IHttpActionResult Actualizar(int id, UsuarioActualizarRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, RespuestaApi.Falla("Datos invalidos", ErroresDe(ModelState)));
            }

            if (UsuarioDAO.ObtenerPorId(id) == null)
            {
                return Content(HttpStatusCode.NotFound, RespuestaApi.Falla("Usuario no encontrado"));
            }

            UsuarioDAO.Actualizar(new Usuario
            {
                Id = id,
                NombreCompleto = request.NombreCompleto,
                RolId = request.RolId,
                Activo = request.Activo
            });

            if (!string.IsNullOrWhiteSpace(request.PasswordNueva))
            {
                UsuarioDAO.ActualizarPassword(id, PasswordHelper.Hash(request.PasswordNueva));
            }

            return Ok(RespuestaApi.Ok(UsuarioDAO.ObtenerPorId(id), "Usuario actualizado"));
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Autorizar(Roles = "ADMIN")]
        public IHttpActionResult Eliminar(int id)
        {
            if (UsuarioDAO.ObtenerPorId(id) == null)
            {
                return Content(HttpStatusCode.NotFound, RespuestaApi.Falla("Usuario no encontrado"));
            }

            UsuarioDAO.Desactivar(id);

            return Ok(RespuestaApi.Ok(null, "Usuario desactivado"));
        }

        private static List<string> ErroresDe(ModelStateDictionary modelState)
        {
            return modelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        }
    }
}
