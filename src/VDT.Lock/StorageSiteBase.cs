using System.Text;

namespace VDT.Lock;

public abstract class StorageSiteBase : IDisposable {
    protected readonly StorageSettings storageSettings;

    public StorageSiteBase(StorageSettings storageSettings) {
        this.storageSettings = storageSettings;
    }

    public abstract SecureBuffer Load();

    public abstract void Save();

    public void SerializeTo(SecureByteList plainBytes) {
        plainBytes.WriteSpan(Encoding.UTF8.GetBytes(GetType().Name));
        storageSettings.SerializeTo(plainBytes);
    }

    public void Dispose() {
        storageSettings.Dispose();
        GC.SuppressFinalize(this);
    }
}
