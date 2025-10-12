namespace VDT.Lock.Services;

public interface IHashProvider {
    SecureBuffer Provide(SecureBuffer plainBuffer, byte[] salt);
}
