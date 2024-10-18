using System;

namespace VDT.Lock;

public static class ReadOnlySpanExtensions {
    public static int ReadInt(this ReadOnlySpan<byte> plainSpan, ref int position) {
        return plainSpan[position++] | (plainSpan[position++] << 8) | (plainSpan[position++] << 16) | (plainSpan[position++] << 24);
    }

    public static ReadOnlySpan<byte> ReadSpan(this ReadOnlySpan<byte> plainSpan, ref int position) {
        var length = ReadInt(plainSpan, ref position);
        var value = plainSpan.Slice(position, length);

        position += length;

        return value;
    }
}
