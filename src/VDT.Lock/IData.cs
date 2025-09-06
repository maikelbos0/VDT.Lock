using System.Collections.Generic;

namespace VDT.Lock;

// TODO: rewrite all implementations to not depend on magic numbers - use BaseData
public interface IData {
    // TODO deserialize could be also static interface method in latest C#

    IEnumerable<int> FieldLengths { get; }

    void SerializeTo(SecureByteList plainBytes);
}
