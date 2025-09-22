using System.Collections.Generic;

namespace VDT.Lock.Tests;

public static class DataProvider {
    public static IEnumerable<byte> CreateSerializedIdentity(byte key)
        => [32, 0, 0, 0, 16, 0, 0, 0, key, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

    public static DataIdentity CreateIdentity(byte key)
        => DataIdentity.DeserializeFrom([16, 0, 0, 0, key, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
}
