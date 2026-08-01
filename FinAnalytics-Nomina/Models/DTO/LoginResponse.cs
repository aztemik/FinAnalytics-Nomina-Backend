namespace FinAnalytics_Nomina.Models.DTO
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string NombreCompleto { get; set; }
        public string Rol { get; set; }
    }
}
