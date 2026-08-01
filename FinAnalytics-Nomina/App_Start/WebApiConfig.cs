using System.Web.Http;
using System.Web.Http.Cors;
using FinAnalytics_Nomina.Handlers;
using Newtonsoft.Json.Serialization;

namespace FinAnalytics_Nomina
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // CORS: origen(es) del cliente (Live Server). Unico lugar a tocar si cambia el
            // puerto/origen. 5501 se agrego para probar en este entorno porque el 5500
            // (el default documentado en PLAN_FRONTEND.md) ya estaba ocupado por VS Code.
            var cors = new EnableCorsAttribute("http://127.0.0.1:5500,http://127.0.0.1:5501", "*", "*");
            config.EnableCors(cors);

            // La API responde solo JSON.
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // Las propiedades C# (PascalCase) se serializan en camelCase, que es el
            // formato que documenta el contrato de la API (PLAN_BACKEND.md §6) y el
            // que espera el frontend (exito, mensaje, datos, errores).
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();

            config.MessageHandlers.Add(new PreflightRequestsHandler());
        }
    }
}
