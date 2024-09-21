namespace VDT.Lock {
    public interface IEncryptor {
        Task<SecureBuffer> Decrypt(SecureBuffer payloadBuffer, byte[] key);
        Task<SecureBuffer> Encrypt(SecureBuffer plainBuffer, byte[] key);
    }
}
