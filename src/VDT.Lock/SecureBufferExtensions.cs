using System;

namespace VDT.Lock;

public static class SecureBufferExtensions {
    public static int ReadInt(this SecureBuffer plainBuffer, ref int position) {
        return plainBuffer.Value[position++] | (plainBuffer.Value[position++] << 8) | (plainBuffer.Value[position++] << 16) | (plainBuffer.Value[position++] << 24);
    }

    public static ReadOnlySpan<byte> ReadSpan(this SecureBuffer plainBuffer, ref int position) {
        var length = ReadInt(plainBuffer, ref position);
        var value = new ReadOnlySpan<byte>(plainBuffer.Value, position, length);

        position += length;

        return value;
    }
}
