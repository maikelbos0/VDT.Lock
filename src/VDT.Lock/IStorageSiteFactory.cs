using System;

namespace VDT.Lock;

public interface IStorageSiteFactory {
    StorageSiteBase DeserializeFrom(ReadOnlySpan<byte> plainSpan);
}
