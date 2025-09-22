using System.Collections.Generic;

namespace VDT.Lock.Tests;

public static class DataProvider {
    public static IEnumerable<byte> CreateSerializedIdentity(byte key)
        => [32, 0, 0, 0, 16, 0, 0, 0, key, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

    public static DataIdentity CreateIdentity(byte key)
        => DataIdentity.DeserializeFrom([16, 0, 0, 0, key, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

    public static IEnumerable<byte> CreateSerializedValue(byte key, params byte[] value)
        => [(byte)(40 + value.Length), 0, 0, 0, .. CreateSerializedIdentity(key), (byte)value.Length, 0, 0, 0, .. value];

    public static DataValue CreateValue(byte key, params byte[] value)
        => new(CreateIdentity(key), value);
}
