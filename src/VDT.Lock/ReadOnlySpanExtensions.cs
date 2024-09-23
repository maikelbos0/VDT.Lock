using System.Text;

namespace VDT.Lock;

public static class ReadOnlySpanExtensions {
    public static int ReadInt(this ReadOnlySpan<byte> span, ref int position) {
        return span[position++] | (span[position++] << 8) | (span[position++] << 16) | (span[position++] << 24);
    }

    public static string ReadString(this ReadOnlySpan<byte> span, ref int position) {
        var length = ReadInt(span, ref position);
        var value = Encoding.UTF8.GetString(span.Slice(position, length));

        position += length;

        return value;
    }

    public static SecureBuffer ReadSecureBuffer(this ReadOnlySpan<byte> span, ref int position) {
        var length = ReadInt(span, ref position);
        var buffer = new SecureBuffer(span.Slice(position, length).ToArray());

        position += length;

        return buffer;
    }
}
