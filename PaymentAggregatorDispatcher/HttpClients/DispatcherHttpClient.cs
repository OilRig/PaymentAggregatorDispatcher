using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentAggregatorDispatcher.HttpClients
{
    public interface IDispatcherHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken);
        Task<HttpResponseMessage> PostAsync(string url, HttpContent content);
    }
    public class DispatcherHttpClient : IDispatcherHttpClient
    {
        private readonly HttpClient _httpClient;

        public DispatcherHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        Task<HttpResponseMessage> IDispatcherHttpClient.PostAsync(string url, HttpContent content) => _httpClient.PostAsync(url, content);

        Task<HttpResponseMessage> IDispatcherHttpClient.SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        => _httpClient.SendAsync(request, completionOption, cancellationToken);
    }
}
