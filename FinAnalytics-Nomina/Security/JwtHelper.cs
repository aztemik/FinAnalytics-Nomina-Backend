using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FinAnalytics_Nomina.Security
{
    public static class JwtHelper
    {
        private static readonly string Secreto = ConfigurationManager.AppSettings["JwtSecret"];
        private static readonly string Emisor = ConfigurationManager.AppSettings["JwtIssuer"];
        private const int HorasExpiracion = 8;

        public static string GenerarToken(int usuarioId, string username, string rol, int? empleadoId)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
                new Claim("username", username),
                new Claim("role", rol)
            };

            if (empleadoId.HasValue)
            {
                claims.Add(new Claim("empleadoId", empleadoId.Value.ToString()));
            }

            var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secreto));
            var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Emisor,
                audience: Emisor,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(HorasExpiracion),
                signingCredentials: credenciales
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static ClaimsPrincipal ValidarToken(string token)
        {
            var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secreto));

            var parametros = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Emisor,
                ValidateAudience = true,
                ValidAudience = Emisor,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = clave,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                // Por defecto, JwtSecurityTokenHandler remapea claims cortos conocidos
                // ("role", "sub") a URIs largas de .NET (ClaimTypes.Role, etc.) al validar.
                // AutorizarAttribute busca el claim tal cual viene en el token ("role"),
                // asi que sin esto FindFirst("role") siempre da null y todo termina en 403,
                // sin importar el rol real del usuario.
                var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
                return handler.ValidateToken(token, parametros, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}
