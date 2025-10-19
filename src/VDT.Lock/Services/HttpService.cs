using System.Net.Http;
using System.Threading.Tasks;

namespace VDT.Lock.Services;

public class HttpService : IHttpService {
    private readonly IHttpClientFactory httpClientFactory;

    public HttpService(IHttpClientFactory httpClientFactory) {
        this.httpClientFactory = httpClientFactory;
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request) {
        var httpClient = httpClientFactory.CreateClient();

        return httpClient.SendAsync(request);
    }
}
