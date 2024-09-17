namespace VDT.Lock;

public interface IHashProvider {
    byte[] Provide(SecureByteArray plainBytes, byte[] salt);
}