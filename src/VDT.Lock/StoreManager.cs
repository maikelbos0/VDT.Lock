namespace VDT.Lock;

public sealed class StoreManager : IDisposable {
    private readonly IEncryptor encryptor;
    private readonly IStorageSiteFactory storageSiteFactory;
    private readonly SecureBuffer plainSessionKeyBuffer;
    private readonly SecureBuffer encryptedStoreKeyBuffer;

    public IList<StorageSiteBase> StorageSites { get; } = [];

    public StoreManager(IEncryptor encryptor, IStorageSiteFactory storageSiteFactory, SecureBuffer plainSessionKeyBuffer, SecureBuffer encryptedStoreKeyBuffer) {
        this.encryptor = encryptor;
        this.storageSiteFactory = storageSiteFactory;
        this.plainSessionKeyBuffer = plainSessionKeyBuffer;
        this.encryptedStoreKeyBuffer = encryptedStoreKeyBuffer;
    }

    public async Task LoadStorageSites(SecureBuffer encryptedStorageSettingsBuffer) {
        using var plainStoreKeyBuffer = await GetPlainStoreKeyBuffer();
        using var plainStorageSettingsBuffer = await encryptor.Decrypt(encryptedStorageSettingsBuffer, plainStoreKeyBuffer);
        var position = 0;

        while (position < plainStorageSettingsBuffer.Value.Length) {
            var storageSiteTypeName = SettingsSerializer.ReadString(plainStorageSettingsBuffer.Value, ref position);
            var storageSettings = new StorageSettings(SettingsSerializer.ReadSpan(plainStorageSettingsBuffer.Value, ref position));
            var storageSite = storageSiteFactory.Create(storageSiteTypeName, storageSettings);

            StorageSites.Add(storageSite);
        }
    }

    public Task<SecureBuffer> GetPlainStoreKeyBuffer() => encryptor.Decrypt(encryptedStoreKeyBuffer, plainSessionKeyBuffer);

    public void Dispose() {
        plainSessionKeyBuffer.Dispose();
        encryptedStoreKeyBuffer.Dispose();
        GC.SuppressFinalize(this);
    }
}
