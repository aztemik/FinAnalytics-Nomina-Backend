using System.ComponentModel.DataAnnotations;

namespace FinAnalytics_Nomina.Models.DTO
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contrasena es obligatoria")]
        public string Password { get; set; }
    }
}
