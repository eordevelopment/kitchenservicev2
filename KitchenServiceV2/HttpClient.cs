using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace KitchenServiceV2
{
    public class HttpClient : IHttpClient
    {
        public Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                return client.GetAsync(requestUri);
            }
        }
    }
}
