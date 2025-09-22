namespace VDT.Lock.Tests;

public static class DataProvider {
    public static byte[] SerializedDataIdentity { get; } = [16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    public static DataIdentity DataIdentity { get; } = DataIdentity.DeserializeFrom(SerializedDataIdentity);
}
