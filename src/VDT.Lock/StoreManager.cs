using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace VDT.Lock;

public sealed class StoreManager : IDisposable {
    public static byte[] MasterPasswordSalt => [66, 6, 86, 3, 238, 211, 38, 177, 32, 98, 112, 223, 115, 234, 230, 103];

    private readonly IEncryptor encryptor;
    private readonly IStorageSiteFactory storageSiteFactory;
    private readonly IRandomByteGenerator randomByteGenerator;
    private readonly IHashProvider hashProvider;

    private SecureBuffer? plainSessionKeyBuffer;
    private SecureBuffer? encryptedStoreKeyBuffer;
    private readonly DataCollection<StorageSiteBase> storageSites = [];

    public bool IsDisposed { get; private set; }

    [MemberNotNullWhen(true, nameof(plainSessionKeyBuffer), nameof(encryptedStoreKeyBuffer))]
    public bool IsAuthenticated {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return encryptedStoreKeyBuffer != null;
        }
    }

    public DataCollection<StorageSiteBase> StorageSites {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return storageSites;
        }
    }

    public StoreManager(IEncryptor encryptor, IStorageSiteFactory storageSiteFactory, IRandomByteGenerator randomByteGenerator, IHashProvider hashProvider) {
        this.encryptor = encryptor;
        this.storageSiteFactory = storageSiteFactory;
        this.randomByteGenerator = randomByteGenerator;
        this.hashProvider = hashProvider;
    }

    public async Task Authenticate(SecureBuffer plainMasterPasswordBuffer) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (IsAuthenticated) {
            plainSessionKeyBuffer.Dispose();
            encryptedStoreKeyBuffer.Dispose();
        }

        using var storeKeyBuffer = hashProvider.Provide(plainMasterPasswordBuffer, MasterPasswordSalt);
        plainSessionKeyBuffer = new SecureBuffer(randomByteGenerator.Generate(Encryptor.KeySizeInBytes));
        encryptedStoreKeyBuffer = await encryptor.Encrypt(storeKeyBuffer, plainSessionKeyBuffer);
    }

    public async Task LoadStorageSites(SecureBuffer encryptedStorageSettingsBuffer) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        EnsureAuthenticated();

        using var plainStoreKeyBuffer = await GetPlainStoreKeyBuffer();
        using var plainStorageSettingsBuffer = await encryptor.Decrypt(encryptedStorageSettingsBuffer, plainStoreKeyBuffer);
        var position = 0;

        while (position < plainStorageSettingsBuffer.Value.Length) {
            var storageSiteTypeName = Encoding.UTF8.GetString(plainStorageSettingsBuffer.ReadSpan(ref position));
            var storageSettings = StorageSettings.DeserializeFrom(plainStorageSettingsBuffer.ReadSpan(ref position));
            var storageSite = storageSiteFactory.Create(storageSiteTypeName, storageSettings);

            StorageSites.Add(storageSite);
        }
    }

    public async Task<SecureBuffer> SaveStorageSites() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        EnsureAuthenticated();

        using var plainStorageSettingsBytes = new SecureByteList();

        foreach (var storageSite in StorageSites) {
            storageSite.SerializeTo(plainStorageSettingsBytes);
        }

        using var plainStoreKeyBuffer = await GetPlainStoreKeyBuffer();
        using var plainStorageSettingsBuffer = plainStorageSettingsBytes.ToBuffer();

        return await encryptor.Encrypt(plainStorageSettingsBuffer, plainStoreKeyBuffer);
    }

    public Task<SecureBuffer> GetPlainStoreKeyBuffer() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        EnsureAuthenticated();

        return encryptor.Decrypt(encryptedStoreKeyBuffer, plainSessionKeyBuffer);
    }

    [MemberNotNull(nameof(plainSessionKeyBuffer), nameof(encryptedStoreKeyBuffer))]
    public void EnsureAuthenticated() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (!IsAuthenticated) {
            throw new NotAuthenticatedException();
        }
    }

    public void Dispose() {
        plainSessionKeyBuffer?.Dispose();
        encryptedStoreKeyBuffer?.Dispose();
        StorageSites.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
