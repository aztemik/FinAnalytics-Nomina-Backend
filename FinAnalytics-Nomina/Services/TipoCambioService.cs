using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using FinAnalytics_Nomina.Data;

namespace FinAnalytics_Nomina.Services
{
    // Resultado de consultar el tipo de cambio: el valor, de donde salio y cuando.
    public class ResultadoTipoCambio
    {
        public decimal Valor { get; set; }
        public string Fuente { get; set; } // API | CACHE
        public DateTime Fecha { get; set; }
        public string Advertencia { get; set; }
    }

    public static class TipoCambioService
    {
        private const string UrlFrankfurter = "https://api.frankfurter.app/latest?from=USD&to=MXN";

        // Cascada: API -> ultimo valor en cache (BD) -> null (el llamador responde 503).
        public static ResultadoTipoCambio Obtener()
        {
            return ObtenerDeApi() ?? ObtenerDeCache();
        }

        private static ResultadoTipoCambio ObtenerDeApi()
        {
            try
            {
                using (var cliente = new HttpClient { Timeout = TimeSpan.FromSeconds(5) })
                {
                    string respuesta = cliente.GetStringAsync(UrlFrankfurter).GetAwaiter().GetResult();
                    var json = JObject.Parse(respuesta);
                    decimal valor = json["rates"]["MXN"].Value<decimal>();

                    return new ResultadoTipoCambio
                    {
                        Valor = valor,
                        Fuente = "API",
                        Fecha = DateTime.Now
                    };
                }
            }
            catch
            {
                // Timeout, sin red, o respuesta inesperada: se intenta la cache.
                return null;
            }
        }

        private static ResultadoTipoCambio ObtenerDeCache()
        {
            var cache = PeriodoDAO.ObtenerUltimoTipoCambioCache();
            if (cache == null)
            {
                return null;
            }

            return new ResultadoTipoCambio
            {
                Valor = cache.Value.Valor,
                Fuente = "CACHE",
                Fecha = cache.Value.Fecha,
                Advertencia = "No se pudo contactar el servicio de tipo de cambio; se uso el ultimo valor conocido."
            };
        }
    }
}
