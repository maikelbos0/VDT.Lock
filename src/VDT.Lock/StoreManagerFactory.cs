namespace VDT.Lock;

public sealed class StoreManagerFactory {
    public static byte[] MasterPasswordSalt => [66, 6, 86, 3, 238, 211, 38, 177, 32, 98, 112, 223, 115, 234, 230, 103];

    // TODO DI
    public async Task<StoreManager> Create(SecureBuffer plainMasterPasswordBuffer) {
        var hashProvider = new HashProvider();
        using var storeKeyBuffer = hashProvider.Provide(plainMasterPasswordBuffer, MasterPasswordSalt);
        var randomByteGenerator = new RandomByteGenerator();
        var encryptor = new Encryptor(randomByteGenerator);
        var plainSessionKeyBuffer = new SecureBuffer(randomByteGenerator.Generate(Encryptor.KeySizeInBytes));
        var encryptedStoreKeyBuffer = await encryptor.Encrypt(storeKeyBuffer, plainSessionKeyBuffer);

        return new StoreManager(encryptor, plainSessionKeyBuffer, encryptedStoreKeyBuffer);
    }
}
