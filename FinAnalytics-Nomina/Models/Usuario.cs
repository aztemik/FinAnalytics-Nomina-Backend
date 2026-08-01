using System;

namespace FinAnalytics_Nomina.Models
{
    // Entidad interna: incluye PasswordHash. Nunca se serializa directo al cliente,
    // para eso se usa Models/DTO/UsuarioDTO.cs.
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string NombreCompleto { get; set; }
        public int RolId { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
