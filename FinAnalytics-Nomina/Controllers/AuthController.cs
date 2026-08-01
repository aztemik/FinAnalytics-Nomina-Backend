using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web.Http;
using FinAnalytics_Nomina.Data;
using FinAnalytics_Nomina.Models.DTO;
using FinAnalytics_Nomina.Security;

namespace FinAnalytics_Nomina.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Content(HttpStatusCode.BadRequest, RespuestaApi.Falla("Datos invalidos", errores));
            }

            var usuario = UsuarioDAO.ObtenerPorUsername(request.Username);

            if (usuario == null || !usuario.Activo || !PasswordHelper.Verificar(request.Password, usuario.PasswordHash))
            {
                return Content(HttpStatusCode.Unauthorized, RespuestaApi.Falla("Credenciales invalidas"));
            }

            var rol = UsuarioDAO.ObtenerNombreRol(usuario.RolId);
            int? empleadoId = null;

            if (rol == "EMPLEADO")
            {
                empleadoId = EmpleadoDAO.ObtenerIdPorUsuarioId(usuario.Id);
            }

            var token = JwtHelper.GenerarToken(usuario.Id, usuario.Username, rol, empleadoId);

            var respuesta = new LoginResponse
            {
                Token = token,
                Username = usuario.Username,
                NombreCompleto = usuario.NombreCompleto,
                Rol = rol
            };

            return Ok(RespuestaApi.Ok(respuesta, "Inicio de sesion exitoso"));
        }

        [HttpGet]
        [Route("me")]
        [Autorizar]
        public IHttpActionResult Me()
        {
            var principal = User as ClaimsPrincipal;

            var datos = new
            {
                id = principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
                username = principal?.FindFirst("username")?.Value,
                rol = principal?.FindFirst("role")?.Value,
                empleadoId = principal?.FindFirst("empleadoId")?.Value
            };

            return Ok(RespuestaApi.Ok(datos));
        }
    }
}
