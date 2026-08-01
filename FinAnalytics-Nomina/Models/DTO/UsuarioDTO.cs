namespace FinAnalytics_Nomina.Models.DTO
{
    // Salida segura de un usuario: nunca incluye PasswordHash.
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string NombreCompleto { get; set; }
        public int RolId { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
    }
}
