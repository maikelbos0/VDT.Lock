namespace VDT.Lock;

// TODO: rewrite all implementations to not depend on magic numbers - use BaseData
public interface IData {
    // TODO deserialize could be also static interface method in latest C#
    int Length { get; }
    void SerializeTo(SecureByteList plainBytes);
}
