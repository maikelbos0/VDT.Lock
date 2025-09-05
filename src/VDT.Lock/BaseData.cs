using System.Linq;

namespace VDT.Lock;

public abstract class BaseData : IData {
    public abstract int[] FieldLengths { get; }

    public int Length => FieldLengths.Sum(field => field + 4);

    public abstract void SerializeTo(SecureByteList plainBytes);
}
