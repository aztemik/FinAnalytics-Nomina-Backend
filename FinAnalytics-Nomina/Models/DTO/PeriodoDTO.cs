using System;

namespace FinAnalytics_Nomina.Models.DTO
{
    public class PeriodoDTO
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; }
        public decimal? TipoCambioUsd { get; set; }
        public string FuenteTipoCambio { get; set; }
        public decimal TotalPercepciones { get; set; }
        public decimal TotalDeducciones { get; set; }
        public decimal TotalNeto { get; set; }
        public decimal TotalCargaPatronal { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaAprobacion { get; set; }
    }
}
