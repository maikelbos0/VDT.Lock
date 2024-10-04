using System.Threading.Tasks;

namespace VDT.Lock {
    public interface IEncryptor {
        Task<SecureBuffer> Decrypt(SecureBuffer payloadBuffer, SecureBuffer keyBuffer);
        Task<SecureBuffer> Encrypt(SecureBuffer plainBuffer, SecureBuffer keyBuffer);
    }
}
