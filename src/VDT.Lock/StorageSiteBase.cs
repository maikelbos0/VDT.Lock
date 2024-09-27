namespace VDT.Lock;

public abstract class StorageSiteBase : IDisposable {
    protected readonly StorageSettings storageSettings;

    public StorageSiteBase(StorageSettings storageSettings) {
        this.storageSettings = storageSettings;
    }

    public abstract SecureBuffer Load();

    public abstract void Save();

    public void SaveTo(SecureByteList plainBytes) {
        using var plainSettingsBuffer = storageSettings.Serialize();

        SettingsSerializer.WriteString(plainBytes, GetType().Name);
        SettingsSerializer.WriteSecureBuffer(plainBytes, plainSettingsBuffer);
    }

    public void Dispose() {
        storageSettings.Dispose();
        GC.SuppressFinalize(this);
    }
}
