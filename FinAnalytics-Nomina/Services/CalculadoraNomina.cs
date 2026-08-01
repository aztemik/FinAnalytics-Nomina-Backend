using System;
using System.Collections.Generic;
using System.Linq;
using FinAnalytics_Nomina.Data;
using FinAnalytics_Nomina.Models.DTO;

namespace FinAnalytics_Nomina.Services
{
    public class ReciboCalculado
    {
        public int EmpleadoId { get; set; }
        public decimal SueldoBase { get; set; }
        public decimal TotalPercepciones { get; set; }
        public decimal TotalDeducciones { get; set; }
        public decimal NetoPagar { get; set; }
        public decimal CargaPatronal { get; set; }
        public List<DetalleReciboDTO> Detalle { get; set; }
    }

    public class ResultadoCalculoNomina
    {
        public List<ReciboCalculado> Recibos { get; set; }
        public decimal TotalPercepciones { get; set; }
        public decimal TotalDeducciones { get; set; }
        public decimal TotalNeto { get; set; }
        public decimal TotalCargaPatronal { get; set; }
        public decimal? TipoCambioUsd { get; set; }
        public string FuenteTipoCambio { get; set; }
    }

    // Hay empleados de honorarios en USD pero no hay tipo de cambio disponible
    // (ni API ni cache). El controlador la traduce a 503.
    public class TipoCambioNoDisponibleException : Exception
    {
        public TipoCambioNoDisponibleException(string mensaje) : base(mensaje) { }
    }

    public static class CalculadoraNomina
    {
        public static ResultadoCalculoNomina Calcular()
        {
            var tasas = ParametroDAO.ObtenerTodos().ToDictionary(p => p.Clave, p => p.Valor);
            var empleados = EmpleadoDAO.ObtenerTodos().Where(e => e.Activo).ToList();

            decimal? tipoCambio = null;
            string fuenteTipoCambio = null;

            bool hayHonorariosUsd = empleados.Any(e => e.TipoContratacion == "HONORARIOS" && e.Moneda == "USD");
            if (hayHonorariosUsd)
            {
                var resultado = TipoCambioService.Obtener();
                if (resultado == null)
                {
                    throw new TipoCambioNoDisponibleException(
                        "No se pudo obtener el tipo de cambio: el servicio externo no respondio y no hay ningun valor en cache. Se requiere captura manual.");
                }

                tipoCambio = resultado.Valor;
                fuenteTipoCambio = resultado.Fuente;
            }

            var recibos = empleados.Select(e => CalcularRecibo(e, tasas, tipoCambio)).ToList();

            return new ResultadoCalculoNomina
            {
                Recibos = recibos,
                TotalPercepciones = recibos.Sum(r => r.TotalPercepciones),
                TotalDeducciones = recibos.Sum(r => r.TotalDeducciones),
                TotalNeto = recibos.Sum(r => r.NetoPagar),
                TotalCargaPatronal = recibos.Sum(r => r.CargaPatronal),
                TipoCambioUsd = tipoCambio,
                FuenteTipoCambio = fuenteTipoCambio
            };
        }

        private static ReciboCalculado CalcularRecibo(EmpleadoDTO empleado, Dictionary<string, decimal> tasas, decimal? tipoCambio)
        {
            return empleado.TipoContratacion == "HONORARIOS"
                ? CalcularHonorarios(empleado, tasas, tipoCambio)
                : CalcularNomina(empleado, tasas);
        }

        private static ReciboCalculado CalcularNomina(EmpleadoDTO empleado, Dictionary<string, decimal> tasas)
        {
            decimal baseCalculo = Redondear(empleado.SalarioMensual / 2);

            decimal isr = Redondear(baseCalculo * tasas["ISR_TASA"]);
            decimal imssObrero = Redondear(baseCalculo * tasas["IMSS_OBRERO"]);
            decimal imssPatronal = Redondear(baseCalculo * tasas["IMSS_PATRONAL"]);
            decimal infonavit = Redondear(baseCalculo * tasas["INFONAVIT"]);
            decimal sar = Redondear(baseCalculo * tasas["SAR"]);
            decimal isn = Redondear(baseCalculo * tasas["ISN"]);

            decimal totalDeducciones = isr + imssObrero;
            decimal cargaPatronal = imssPatronal + infonavit + sar + isn;

            return new ReciboCalculado
            {
                EmpleadoId = empleado.Id,
                SueldoBase = baseCalculo,
                TotalPercepciones = baseCalculo,
                TotalDeducciones = totalDeducciones,
                NetoPagar = baseCalculo - totalDeducciones,
                CargaPatronal = cargaPatronal,
                Detalle = new List<DetalleReciboDTO>
                {
                    new DetalleReciboDTO { Concepto = "Sueldo quincenal", Tipo = "PERCEPCION", Monto = baseCalculo },
                    new DetalleReciboDTO { Concepto = "ISR", Tipo = "DEDUCCION", Monto = isr },
                    new DetalleReciboDTO { Concepto = "IMSS obrero", Tipo = "DEDUCCION", Monto = imssObrero },
                    new DetalleReciboDTO { Concepto = "IMSS patronal", Tipo = "PATRONAL", Monto = imssPatronal },
                    new DetalleReciboDTO { Concepto = "INFONAVIT", Tipo = "PATRONAL", Monto = infonavit },
                    new DetalleReciboDTO { Concepto = "SAR", Tipo = "PATRONAL", Monto = sar },
                    new DetalleReciboDTO { Concepto = "ISN", Tipo = "PATRONAL", Monto = isn }
                }
            };
        }

        private static ReciboCalculado CalcularHonorarios(EmpleadoDTO empleado, Dictionary<string, decimal> tasas, decimal? tipoCambio)
        {
            decimal baseCalculo = empleado.Moneda == "USD"
                ? Redondear(empleado.SalarioMensual * tipoCambio.Value)
                : empleado.SalarioMensual;

            decimal retIsr = Redondear(baseCalculo * tasas["RET_ISR_HON"]);
            decimal retIva = Redondear(baseCalculo * tasas["RET_IVA_HON"]);
            decimal totalDeducciones = retIsr + retIva;

            return new ReciboCalculado
            {
                EmpleadoId = empleado.Id,
                SueldoBase = baseCalculo,
                TotalPercepciones = baseCalculo,
                TotalDeducciones = totalDeducciones,
                NetoPagar = baseCalculo - totalDeducciones,
                CargaPatronal = 0,
                Detalle = new List<DetalleReciboDTO>
                {
                    new DetalleReciboDTO { Concepto = "Honorarios", Tipo = "PERCEPCION", Monto = baseCalculo },
                    new DetalleReciboDTO { Concepto = "Retencion ISR", Tipo = "DEDUCCION", Monto = retIsr },
                    new DetalleReciboDTO { Concepto = "Retencion IVA", Tipo = "DEDUCCION", Monto = retIva }
                }
            };
        }

        private static decimal Redondear(decimal valor)
        {
            return Math.Round(valor, 2, MidpointRounding.AwayFromZero);
        }
    }
}
