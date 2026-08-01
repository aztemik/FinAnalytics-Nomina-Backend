using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FinAnalytics_Nomina.Handlers
{
    // No intercepta OPTIONS: contestarlo aqui con un 200 "pelado" (sin headers CORS)
    // rompia el preflight -- el navegador lo descarta por faltarle Access-Control-Allow-Origin
    // y nunca llega a mandar la peticion real. El unico lugar que configura CORS es
    // config.EnableCors(...) en WebApiConfig.cs (ver PLAN_BACKEND.md gotcha #1: CORS se
    // configura en un solo lugar); ese mecanismo ya responde el preflight correctamente,
    // con los headers Access-Control-* que le corresponden segun el origen.
    public class PreflightRequestsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }
    }
}
