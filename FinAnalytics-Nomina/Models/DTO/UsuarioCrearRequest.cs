using System.ComponentModel.DataAnnotations;

namespace FinAnalytics_Nomina.Models.DTO
{
    public class UsuarioCrearRequest
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        [RegularExpression(@"^\S{4,50}$", ErrorMessage = "El usuario debe tener entre 4 y 50 caracteres, sin espacios")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contrasena es obligatoria")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[0-9]).{8,}$", ErrorMessage = "La contrasena debe tener minimo 8 caracteres, una mayuscula y un numero")]
        public string Password { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(120, ErrorMessage = "El nombre completo es demasiado largo")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Rol invalido")]
        public int RolId { get; set; }
    }
}
