namespace VDT.Lock.Services;

public class StorageSiteServices : IStorageSiteServices {
    public IFileService FileService { get; set; } = new FileService();
}
