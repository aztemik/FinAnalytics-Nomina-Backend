namespace FinAnalytics_Nomina.Models.DTO
{
    public class DetalleReciboDTO
    {
        public string Concepto { get; set; }
        public string Tipo { get; set; } // PERCEPCION | DEDUCCION | PATRONAL
        public decimal Monto { get; set; }
    }
}
