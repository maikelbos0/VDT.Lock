namespace VDT.Lock;

public interface IHashProvider {
    SecureBuffer Provide(SecureBuffer plainBuffer, byte[] salt);
}
