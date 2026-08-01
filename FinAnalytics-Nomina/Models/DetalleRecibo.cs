namespace FinAnalytics_Nomina.Models
{
    public class DetalleRecibo
    {
        public int Id { get; set; }
        public int ReciboId { get; set; }
        public string Concepto { get; set; }
        public string Tipo { get; set; } // PERCEPCION | DEDUCCION | PATRONAL
        public decimal Monto { get; set; }
    }
}
