using System.ComponentModel.DataAnnotations;

namespace FinAnalytics_Nomina.Models.DTO
{
    public class ParametroActualizarRequest
    {
        [Required(ErrorMessage = "El valor es obligatorio")]
        [Range(0.00001, 1, ErrorMessage = "El valor debe estar entre 0 y 1")]
        public decimal Valor { get; set; }
    }
}
