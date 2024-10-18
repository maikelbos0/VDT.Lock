namespace VDT.Lock;

public interface IData {
    int Length { get; }
    void SerializeTo(SecureByteList plainBytes);
}
