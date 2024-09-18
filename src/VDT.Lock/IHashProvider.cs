namespace VDT.Lock;

public interface IHashProvider {
    byte[] Provide(SecureByteList plainBytes, byte[] salt);
}