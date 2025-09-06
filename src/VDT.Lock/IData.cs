using System;
using System.Collections.Generic;

namespace VDT.Lock;

public interface IData<TSelf> where TSelf : IData<TSelf> {
    static abstract TSelf DeserializeFrom(ReadOnlySpan<byte> plainSpan);

    IEnumerable<int> FieldLengths { get; }

    void SerializeTo(SecureByteList plainBytes);
}
