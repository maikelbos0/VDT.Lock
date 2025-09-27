using System.Collections.Generic;

namespace VDT.Lock;

// TODO combine when complete
public interface IIdentifiableData {
    DataIdentity Identity { get; }
}

public interface IIdentifiableData<TSelf> : IIdentifiableData where TSelf : IIdentifiableData<TSelf> {
    static abstract TSelf Merge(IEnumerable<TSelf> candidates);
}
