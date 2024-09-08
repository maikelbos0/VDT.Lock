namespace VDT.Lock;

public interface IHashProvider {
    byte[] Provide(Stream stream, byte[] salt);
}