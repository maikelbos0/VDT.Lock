using System.Collections.Generic;

namespace VDT.Lock.Tests;

public static class DataProvider {
    public static IEnumerable<byte> CreateSerializedIdentity(byte key)
        => [32, 0, 0, 0, 16, 0, 0, 0, key, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

    public static DataIdentity CreateIdentity(byte key)
        => DataIdentity.DeserializeFrom([16, 0, 0, 0, key, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

    public static DataIdentity CreateIdentity(byte key, byte version)
        => DataIdentity.DeserializeFrom([16, 0, 0, 0, key, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, version, 0, 0, 0, 0, 0, 0, 0]);

    public static IEnumerable<byte> CreateSerializedValue(byte key, byte[] value)
        => [(byte)(40 + value.Length), 0, 0, 0, .. CreateSerializedIdentity(key), (byte)value.Length, 0, 0, 0, .. value];

    public static DataValue CreateValue(byte key, byte[] value)
        => new(CreateIdentity(key), value);

    public static IEnumerable<byte> CreateSerializedField(byte key, byte[] name, byte[] data)
        => [(byte)(48 + name.Length + data.Length), 0, 0, 0, .. CreateSerializedIdentity(key), (byte)name.Length, 0, 0, 0, .. name, (byte)data.Length, 0, 0, 0, .. data, 0, 0, 0, 0];

    public static DataField CreateField(byte key, byte[] name, byte[] data)
        => new(CreateIdentity(key), name, data);

    public static IEnumerable<byte> CreateSerializedItem(byte key, byte[] name)
        => [(byte)(56 + name.Length), 0, 0, 0, .. CreateSerializedIdentity(key), (byte)name.Length, 0, 0, 0, .. name, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

    public static DataItem CreateItem(byte key, byte[] name)
        => new(CreateIdentity(key), name);
}
