namespace VDT.Lock;

public sealed class StoreManager : IDisposable {
    private readonly IEncryptor encryptor;
    private readonly SecureBuffer plainSessionKeyBuffer;
    private readonly SecureBuffer encryptedStoreKeyBuffer;

    public StoreManager(IEncryptor encryptor, SecureBuffer plainSessionKeyBuffer, SecureBuffer encryptedStoreKeyBuffer) {
        this.encryptor = encryptor;
        this.plainSessionKeyBuffer = plainSessionKeyBuffer;
        this.encryptedStoreKeyBuffer = encryptedStoreKeyBuffer;
    }

    public Task<SecureBuffer> GetPlainStoreKeyBuffer() => encryptor.Decrypt(encryptedStoreKeyBuffer, plainSessionKeyBuffer);

    public void Dispose() {
        plainSessionKeyBuffer.Dispose();
        encryptedStoreKeyBuffer.Dispose();
        GC.SuppressFinalize(this);
    }
}
