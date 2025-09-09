using System.Linq;

namespace VDT.Lock;

public static class DataExtensions {
    // TODO should be converted to an extension property when moving to .net 10
    public static int GetLength<TSelf>(this IData<TSelf> data) where TSelf : IData<TSelf>
        => data.FieldLengths.Sum(field => field + 4);
}
