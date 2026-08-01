using System.Collections.Generic;

namespace FinAnalytics_Nomina.Models.DTO
{
    // Envoltorio uniforme de todas las respuestas de la API.
    public class RespuestaApi
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public object Datos { get; set; }
        public List<string> Errores { get; set; }

        public static RespuestaApi Ok(object datos, string mensaje = "Operacion exitosa")
        {
            return new RespuestaApi { Exito = true, Mensaje = mensaje, Datos = datos };
        }

        public static RespuestaApi Falla(string mensaje, List<string> errores = null)
        {
            return new RespuestaApi { Exito = false, Mensaje = mensaje, Errores = errores };
        }
    }
}
