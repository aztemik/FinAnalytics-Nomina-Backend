using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinAnalytics_Nomina.Models.DTO
{
    // Usado tanto para alta (POST) como para edicion (PUT) de empleados.
    public class EmpleadoRequest : IValidatableObject
    {
        [Required(ErrorMessage = "El numero de empleado es obligatorio")]
        [StringLength(20, ErrorMessage = "El numero de empleado es demasiado largo")]
        public string NumeroEmpleado { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(80, ErrorMessage = "El nombre es demasiado largo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        [StringLength(80, ErrorMessage = "Los apellidos son demasiado largos")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "El RFC es obligatorio")]
        [RegularExpression(@"^[A-ZÑ&]{3,4}[0-9]{6}[A-Z0-9]{3}$", ErrorMessage = "El RFC no tiene un formato valido")]
        public string Rfc { get; set; }

        [StringLength(80, ErrorMessage = "El puesto es demasiado largo")]
        public string Puesto { get; set; }

        [StringLength(80, ErrorMessage = "El departamento es demasiado largo")]
        public string Departamento { get; set; }

        [Required(ErrorMessage = "El tipo de contratacion es obligatorio")]
        [RegularExpression("^(NOMINA|HONORARIOS)$", ErrorMessage = "El tipo de contratacion debe ser NOMINA u HONORARIOS")]
        public string TipoContratacion { get; set; }

        [RegularExpression("^(MXN|USD)$", ErrorMessage = "La moneda debe ser MXN o USD")]
        public string Moneda { get; set; } = "MXN";

        [Required(ErrorMessage = "El salario mensual es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El salario debe ser mayor a 0")]
        public decimal SalarioMensual { get; set; }

        [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
        public DateTime FechaIngreso { get; set; }

        public int? UsuarioId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaIngreso.Date > DateTime.Today)
            {
                yield return new ValidationResult(
                    "La fecha de ingreso no puede ser futura",
                    new[] { nameof(FechaIngreso) });
            }
        }
    }
}
