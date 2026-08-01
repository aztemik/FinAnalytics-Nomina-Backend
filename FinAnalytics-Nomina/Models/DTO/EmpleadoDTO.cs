using System;

namespace FinAnalytics_Nomina.Models.DTO
{
    public class EmpleadoDTO
    {
        public int Id { get; set; }
        public string NumeroEmpleado { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Rfc { get; set; }
        public string Puesto { get; set; }
        public string Departamento { get; set; }
        public string TipoContratacion { get; set; }
        public string Moneda { get; set; }
        public decimal SalarioMensual { get; set; }
        public DateTime FechaIngreso { get; set; }
        public int? UsuarioId { get; set; }
        public bool Activo { get; set; }
    }
}
