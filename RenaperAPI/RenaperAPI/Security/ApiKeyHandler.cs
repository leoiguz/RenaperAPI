using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RenaperAPI // Asegúrate que este namespace coincida con el tuyo
{
    public class ApiKeyHandler : DelegatingHandler
    {
        private readonly string API_KEY_SECRETA = ConfigurationManager.AppSettings["RenaperApiKey"];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool contieneHeader = request.Headers.TryGetValues("X-API-KEY", out IEnumerable<string> headerValues);

            if (!contieneHeader)
            {
                return request.CreateResponse(HttpStatusCode.Unauthorized, "Falta la API Key.");
            }

            string apiKeyEnviada = headerValues.FirstOrDefault();

            if (!API_KEY_SECRETA.Equals(apiKeyEnviada))
            {
                return request.CreateResponse(HttpStatusCode.Forbidden, "API Key inválida.");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}