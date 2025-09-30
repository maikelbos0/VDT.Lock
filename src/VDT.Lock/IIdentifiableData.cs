using System.Collections.Generic;

namespace VDT.Lock;

public interface IIdentifiableData<TSelf> where TSelf : IIdentifiableData<TSelf> {
    DataIdentity Identity { get; }
    static abstract TSelf Merge(IEnumerable<TSelf> candidates);
}
