namespace VDT.Lock;

public interface IStorageSiteFactory {
    StorageSiteBase Create(string typeName, StorageSettings settings);
}
