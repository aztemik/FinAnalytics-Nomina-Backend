using System;

namespace FinAnalytics_Nomina.Models
{
    public class Empleado
    {
        public int Id { get; set; }
        public string NumeroEmpleado { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Rfc { get; set; }
        public string Puesto { get; set; }
        public string Departamento { get; set; }
        public string TipoContratacion { get; set; } // NOMINA | HONORARIOS
        public string Moneda { get; set; } // MXN | USD
        public decimal SalarioMensual { get; set; }
        public DateTime FechaIngreso { get; set; }
        public int? UsuarioId { get; set; }
        public bool Activo { get; set; }
    }
}
