using System.Net.Http;
using System.Threading.Tasks;

namespace VDT.Lock.Services;

public interface IHttpService {
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
}
