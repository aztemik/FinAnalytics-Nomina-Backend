using System.Collections.Generic;

namespace FinAnalytics_Nomina.Models.DTO
{
    public class ReciboDTO
    {
        public int Id { get; set; }
        public int PeriodoId { get; set; }
        public int EmpleadoId { get; set; }
        public string NumeroEmpleado { get; set; }
        public string NombreEmpleado { get; set; }
        public decimal SueldoBase { get; set; }
        public decimal TotalPercepciones { get; set; }
        public decimal TotalDeducciones { get; set; }
        public decimal NetoPagar { get; set; }
        public decimal CargaPatronal { get; set; }
        public List<DetalleReciboDTO> Detalle { get; set; }
    }
}
