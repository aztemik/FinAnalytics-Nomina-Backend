namespace FinAnalytics_Nomina.Models
{
    public class ReciboNomina
    {
        public int Id { get; set; }
        public int PeriodoId { get; set; }
        public int EmpleadoId { get; set; }
        public decimal SueldoBase { get; set; }
        public decimal TotalPercepciones { get; set; }
        public decimal TotalDeducciones { get; set; }
        public decimal NetoPagar { get; set; }
        public decimal CargaPatronal { get; set; }
    }
}
