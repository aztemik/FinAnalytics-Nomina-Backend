using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace FinAnalytics_Nomina.Handlers
{
    public class PreflightRequestsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Options)
            {
                return Task.FromResult(request.CreateResponse(HttpStatusCode.OK));
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
