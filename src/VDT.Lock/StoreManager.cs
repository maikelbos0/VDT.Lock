using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace VDT.Lock;

public sealed class StoreManager : IDisposable {
    public static byte[] MasterPasswordSalt => [66, 6, 86, 3, 238, 211, 38, 177, 32, 98, 112, 223, 115, 234, 230, 103];

    private readonly IEncryptor encryptor;
    private readonly IRandomByteGenerator randomByteGenerator;
    private readonly IHashProvider hashProvider;

    private SecureBuffer? plainSessionKeyBuffer;
    private SecureBuffer? encryptedStoreKeyBuffer;

    // TODO should these be encrypted in any way? We have the technology.
    private DataCollection<StorageSiteBase> storageSites = [];

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
        set {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            storageSites.Dispose();
            storageSites = value;
        }
    }

    public StoreManager(IEncryptor encryptor, IRandomByteGenerator randomByteGenerator, IHashProvider hashProvider) {
        this.encryptor = encryptor;
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

    public async Task LoadStorageSites(SecureBuffer encryptedBuffer) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        EnsureAuthenticated();

        try {
            using var plainStoreKeyBuffer = await GetPlainStoreKeyBuffer();
            using var plainBuffer = await encryptor.Decrypt(encryptedBuffer, plainStoreKeyBuffer);
            var position = 0;

            StorageSites = DataCollection<StorageSiteBase>.DeserializeFrom(plainBuffer);
        }
        catch (Exception ex) {
            throw new InvalidAuthenticationException("Deserializing buffer failed.", ex);
        }
    }

    public async Task<SecureBuffer> SaveStorageSites() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        EnsureAuthenticated();

        using var plainStorageSettingsBytes = new SecureByteList();

        storageSites.SerializeTo(plainStorageSettingsBytes, false);

        using var plainStoreKeyBuffer = await GetPlainStoreKeyBuffer();
        using var plainStorageSettingsBuffer = plainStorageSettingsBytes.ToBuffer();

        return await encryptor.Encrypt(plainStorageSettingsBuffer, plainStoreKeyBuffer);
    }

    public async Task<DataStore> LoadDataStore() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        EnsureAuthenticated();

        if (storageSites.Count == 0) {
            return new DataStore();
        }
        else if (storageSites.Count == 1) {
            try {
                using var plainStoreKeyBuffer = await GetPlainStoreKeyBuffer();
                using var encryptedBuffer = await storageSites.Single().Load();
                using var plainBuffer = await encryptor.Decrypt(encryptedBuffer, plainStoreKeyBuffer);
                var position = 0;

                return DataStore.DeserializeFrom(plainBuffer.ReadSpan(ref position));
            }
            catch (Exception ex) {
                throw new InvalidAuthenticationException("Deserializing buffer failed.", ex);
            }
        }
        else {
            throw new InvalidOperationException("Retrieving data store from multiple sites is not yet supported.");
        }
    }

    public async Task SaveDataStore(DataStore dataStore) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        EnsureAuthenticated();

        using var plainBytes = new SecureByteList();

        dataStore.SerializeTo(plainBytes);

        using var plainStoreKeyBuffer = await GetPlainStoreKeyBuffer();
        using var plainBuffer = plainBytes.ToBuffer();
        using var encryptedBuffer = await encryptor.Encrypt(plainBuffer, plainStoreKeyBuffer);

        foreach (var storageSite in storageSites) {
            await storageSite.Save(encryptedBuffer.Value);
        }
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
            throw new NotAuthenticatedException("This method requires authentication.");
        }
    }

    public void Dispose() {
        plainSessionKeyBuffer?.Dispose();
        encryptedStoreKeyBuffer?.Dispose();
        storageSites.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
