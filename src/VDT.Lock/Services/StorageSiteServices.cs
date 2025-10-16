using System.Net.Http;

namespace VDT.Lock.Services;

public class StorageSiteServices : IStorageSiteServices {
    public IFileService FileService { get; }
    public IHttpService HttpService { get; }

    public StorageSiteServices(IHttpClientFactory httpClientFactory) {
        FileService = new FileService();
        HttpService = new HttpService(httpClientFactory);
    }
}
