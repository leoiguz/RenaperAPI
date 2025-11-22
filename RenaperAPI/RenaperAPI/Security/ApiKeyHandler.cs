using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace RenaperAPI // Asegúrate que este namespace coincida con el tuyo
{
    public class ApiKeyHandler : DelegatingHandler
    {
        // Esta es tu clave secreta. En un proyecto real, esto iría en el Web.config
        private const string API_KEY_SECRETA = "renaper-12345-seguro";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. Validamos si existe el Header "X-API-KEY" en la petición
            bool contieneHeader = request.Headers.TryGetValues("X-API-KEY", out IEnumerable<string> headerValues);

            if (!contieneHeader)
            {
                // Si no tiene el header, devolvemos error 401 (No autorizado)
                return request.CreateResponse(HttpStatusCode.Unauthorized, "Falta la API Key.");
            }

            // 2. Obtenemos el valor que enviaron
            string apiKeyEnviada = headerValues.FirstOrDefault();

            // 3. Comparamos con nuestra clave secreta
            if (!API_KEY_SECRETA.Equals(apiKeyEnviada))
            {
                // Si la clave es incorrecta, devolvemos error 403 (Prohibido)
                return request.CreateResponse(HttpStatusCode.Forbidden, "API Key inválida.");
            }

            // 4. Si todo está bien, dejamos pasar la petición al Controller
            return await base.SendAsync(request, cancellationToken);
        }
    }
}