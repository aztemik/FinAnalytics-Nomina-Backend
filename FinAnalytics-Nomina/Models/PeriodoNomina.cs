using System;

namespace FinAnalytics_Nomina.Models
{
    public class PeriodoNomina
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; } // BORRADOR | APROBADO
        public decimal? TipoCambioUsd { get; set; }
        public string FuenteTipoCambio { get; set; } // API | CACHE | MANUAL
        public decimal TotalPercepciones { get; set; }
        public decimal TotalDeducciones { get; set; }
        public decimal TotalNeto { get; set; }
        public decimal TotalCargaPatronal { get; set; }
        public int? CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int? AprobadoPor { get; set; }
        public DateTime? FechaAprobacion { get; set; }
    }
}
