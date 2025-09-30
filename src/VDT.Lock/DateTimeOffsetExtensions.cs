using System;

namespace VDT.Lock;

public static class DateTimeOffsetExtensions {
    public static byte[] ToVersion(this DateTimeOffset dateTimeOffset) {
        var timestamp = dateTimeOffset.ToUnixTimeSeconds();

        unchecked {
            return [
                (byte)timestamp,
                (byte)(timestamp >> 8),
                (byte)(timestamp >> 16),
                (byte)(timestamp >> 24),
                (byte)(timestamp >> 32),
                (byte)(timestamp >> 40),
                (byte)(timestamp >> 48),
                (byte)(timestamp >> 56)
            ];
        }
    } 
}
