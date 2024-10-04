using System;

namespace VDT.Lock;

public static class SecureByteListExtensions {
    public static void WriteInt(this SecureByteList plainBytes, int value) {
        unchecked {
            plainBytes.Add((byte)value);
            plainBytes.Add((byte)(value >> 8));
            plainBytes.Add((byte)(value >> 16));
            plainBytes.Add((byte)(value >> 24));
        }
    }

    public static void WriteSpan(this SecureByteList plainBytes, ReadOnlySpan<byte> plainSpan) {
        WriteInt(plainBytes, plainSpan.Length);
        plainBytes.Add(plainSpan);
    }

    public static void WriteSecureBuffer(this SecureByteList plainBytes, SecureBuffer plainBuffer)
        => WriteSpan(plainBytes, new ReadOnlySpan<byte>(plainBuffer.Value));
}
