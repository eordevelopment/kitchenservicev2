using System.Net.Http;
using System.Threading.Tasks;

namespace KitchenServiceV2
{
    public class HttpClient : IHttpClient
    {
        private readonly System.Net.Http.HttpClient _httpClient;

        public HttpClient()
        {
            this._httpClient = new System.Net.Http.HttpClient();
        }

        public Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return this._httpClient.GetAsync(requestUri);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
