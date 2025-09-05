namespace VDT.Lock;

// TODO: rewrite all implementations to not depend on magic numbers - using SecureBuffer.Length / SecureBuffer.SerializeTo which includes the additional 4 bytes
public interface IData {
    // TODO: rename to size
    int Length { get; }    
    void SerializeTo(SecureByteList plainBytes);
}
