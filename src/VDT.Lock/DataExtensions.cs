using System.Linq;

namespace VDT.Lock;

public static class DataExtensions {
    extension<TSelf>(IData<TSelf> data) where TSelf : IData<TSelf> {
        public int Length => data.FieldLengths.Sum(fieldLength => fieldLength + 4);
    }
}
