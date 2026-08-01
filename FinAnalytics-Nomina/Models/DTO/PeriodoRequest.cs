using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinAnalytics_Nomina.Models.DTO
{
    // Usado tanto para alta (POST) como para edicion (PUT) de periodos.
    public class PeriodoRequest : IValidatableObject
    {
        [Required(ErrorMessage = "La descripcion es obligatoria")]
        [StringLength(100, ErrorMessage = "La descripcion es demasiado larga")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        public DateTime FechaFin { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaFin <= FechaInicio)
            {
                yield return new ValidationResult(
                    "La fecha de fin debe ser posterior a la fecha de inicio",
                    new[] { nameof(FechaFin) });
            }
        }
    }
}
