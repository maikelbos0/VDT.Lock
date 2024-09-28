﻿using System.Diagnostics.CodeAnalysis;

namespace VDT.Lock;

public sealed class StoreManager : IDisposable {
    public static byte[] MasterPasswordSalt => [66, 6, 86, 3, 238, 211, 38, 177, 32, 98, 112, 223, 115, 234, 230, 103];

    private readonly IEncryptor encryptor;
    private readonly IStorageSiteFactory storageSiteFactory;
    private readonly IRandomByteGenerator randomByteGenerator;
    private readonly IHashProvider hashProvider;

    private SecureBuffer? plainSessionKeyBuffer;
    private SecureBuffer? encryptedStoreKeyBuffer;

    [MemberNotNullWhen(true, nameof(plainSessionKeyBuffer), nameof(encryptedStoreKeyBuffer))]
    public bool IsAuthenticated => encryptedStoreKeyBuffer != null;

    public IList<StorageSiteBase> StorageSites { get; } = [];

    public StoreManager(IEncryptor encryptor, IStorageSiteFactory storageSiteFactory, IRandomByteGenerator randomByteGenerator, IHashProvider hashProvider) {
        this.encryptor = encryptor;
        this.storageSiteFactory = storageSiteFactory;
        this.randomByteGenerator = randomByteGenerator;
        this.hashProvider = hashProvider;
    }

    public async Task Authenticate(SecureBuffer plainMasterPasswordBuffer) {
        if (IsAuthenticated) {
            plainSessionKeyBuffer.Dispose();
            encryptedStoreKeyBuffer.Dispose();
        }

        using var storeKeyBuffer = hashProvider.Provide(plainMasterPasswordBuffer, MasterPasswordSalt);
        plainSessionKeyBuffer = new SecureBuffer(randomByteGenerator.Generate(Encryptor.KeySizeInBytes));
        encryptedStoreKeyBuffer = await encryptor.Encrypt(storeKeyBuffer, plainSessionKeyBuffer);
    }

    public async Task LoadStorageSites(SecureBuffer encryptedStorageSettingsBuffer) {
        EnsureAuthenticated();

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

    public async Task<SecureBuffer> SaveStorageSites() {
        EnsureAuthenticated();

        using var plainStorageSettingsBytes = new SecureByteList();

        foreach (var storageSite in StorageSites) {
            storageSite.SaveTo(plainStorageSettingsBytes);
        }

        using var plainStoreKeyBuffer = await GetPlainStoreKeyBuffer();
        using var plainStorageSettingsBuffer = plainStorageSettingsBytes.ToBuffer();

        return await encryptor.Encrypt(plainStorageSettingsBuffer, plainStoreKeyBuffer);
    }

    public Task<SecureBuffer> GetPlainStoreKeyBuffer() {
        EnsureAuthenticated();

        return encryptor.Decrypt(encryptedStoreKeyBuffer, plainSessionKeyBuffer);
    }

    [MemberNotNull(nameof(plainSessionKeyBuffer), nameof(encryptedStoreKeyBuffer))]
    public void EnsureAuthenticated() {
        if (!IsAuthenticated) {
            throw new NotAuthenticatedException();
        }
    }

    public void Dispose() {
        plainSessionKeyBuffer?.Dispose();
        encryptedStoreKeyBuffer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
