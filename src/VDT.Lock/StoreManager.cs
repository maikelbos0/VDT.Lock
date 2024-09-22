namespace VDT.Lock;

public sealed class StoreManager : IDisposable {
    private readonly IEncryptor encryptor;
    private readonly SecureBuffer sessionKeyBuffer;
    private readonly SecureBuffer encryptedStoreKeyBuffer;

    public StoreManager(IEncryptor encryptor, SecureBuffer sessionKeyBuffer, SecureBuffer encryptedStoreKeyBuffer) {
        this.encryptor = encryptor;
        this.sessionKeyBuffer = sessionKeyBuffer;
        this.encryptedStoreKeyBuffer = encryptedStoreKeyBuffer;
    }

    public Task<SecureBuffer> GetStoreKey() => encryptor.Decrypt(encryptedStoreKeyBuffer, sessionKeyBuffer);

    public void Dispose() {
        sessionKeyBuffer.Dispose();
        encryptedStoreKeyBuffer.Dispose();
        GC.SuppressFinalize(this);
    }
}
