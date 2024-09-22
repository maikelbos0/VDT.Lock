namespace VDT.Lock;

public sealed class StoreManagerFactory {
    public static byte[] MasterPasswordSalt => [66, 6, 86, 3, 238, 211, 38, 177, 32, 98, 112, 223, 115, 234, 230, 103];

    // TODO DI
    public async Task<StoreManager> Create(SecureBuffer masterPasswordBuffer) {
        var hashProvider = new HashProvider();
        using var storeKeyBuffer = hashProvider.Provide(masterPasswordBuffer, MasterPasswordSalt);
        var randomByteGenerator = new RandomByteGenerator();
        var encryptor = new Encryptor(randomByteGenerator);
        var sessionKeyBuffer = new SecureBuffer(randomByteGenerator.Generate(Encryptor.KeySizeInBytes));
        var encryptedStoreKeyBuffer = await encryptor.Encrypt(storeKeyBuffer, sessionKeyBuffer);

        return new StoreManager(encryptor, sessionKeyBuffer, encryptedStoreKeyBuffer);
    }
}
