using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace KitchenServiceV2
{
    public interface IHttpClient : IDisposable
    {
        Task<HttpResponseMessage> GetAsync(string requestUri);
    }
}
