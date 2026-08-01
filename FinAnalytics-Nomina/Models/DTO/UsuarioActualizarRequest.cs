using System.ComponentModel.DataAnnotations;

namespace FinAnalytics_Nomina.Models.DTO
{
    public class UsuarioActualizarRequest
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(120, ErrorMessage = "El nombre completo es demasiado largo")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Rol invalido")]
        public int RolId { get; set; }

        public bool Activo { get; set; }

        // Opcional: si viene, el controlador la rehashea. Si se omite, no se toca la contrasena actual.
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[0-9]).{8,}$", ErrorMessage = "La contrasena debe tener minimo 8 caracteres, una mayuscula y un numero")]
        public string PasswordNueva { get; set; }
    }
}
