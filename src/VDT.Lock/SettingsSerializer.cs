using System.Text;

namespace VDT.Lock;

public static class SettingsSerializer {
    public static int ReadInt(ReadOnlySpan<byte> plainSettingsSpan, ref int position) {
        return plainSettingsSpan[position++] | (plainSettingsSpan[position++] << 8) | (plainSettingsSpan[position++] << 16) | (plainSettingsSpan[position++] << 24);
    }

    public static string ReadString(ReadOnlySpan<byte> plainSettingsSpan, ref int position) {
        var length = ReadInt(plainSettingsSpan, ref position);
        var plainText = Encoding.UTF8.GetString(plainSettingsSpan.Slice(position, length));

        position += length;

        return plainText;
    }

    public static SecureBuffer ReadSecureBuffer(ReadOnlySpan<byte> plainSettingsSpan, ref int position) {
        var length = ReadInt(plainSettingsSpan, ref position);
        var plainBuffer = new SecureBuffer(plainSettingsSpan.Slice(position, length).ToArray());

        position += length;

        return plainBuffer;
    }

    public static void WriteInt(SecureByteList plainSettingsBytes, int value) {
        unchecked {
            plainSettingsBytes.Add((byte)value);
            plainSettingsBytes.Add((byte)(value >> 8));
            plainSettingsBytes.Add((byte)(value >> 16));
            plainSettingsBytes.Add((byte)(value >> 24));
        }
    }

    public static void WriteString(SecureByteList plainSettingsBytes, string value) {
        var plainBytes = Encoding.UTF8.GetBytes(value);

        WriteInt(plainSettingsBytes, plainBytes.Length);
        plainSettingsBytes.Add(new ReadOnlySpan<byte>(plainBytes));
    }

    public static void WriteSecureBuffer(SecureByteList plainSettingsBytes, SecureBuffer plainBuffer) {
        WriteInt(plainSettingsBytes, plainBuffer.Value.Length);
        plainSettingsBytes.Add(new ReadOnlySpan<byte>(plainBuffer.Value));
    }
}
