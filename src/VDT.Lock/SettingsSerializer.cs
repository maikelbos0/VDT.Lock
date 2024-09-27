using System.Text;

namespace VDT.Lock;

public static class SettingsSerializer {
    public static int ReadInt(ReadOnlySpan<byte> plainSettingsSpan, ref int position) {
        return plainSettingsSpan[position++] | (plainSettingsSpan[position++] << 8) | (plainSettingsSpan[position++] << 16) | (plainSettingsSpan[position++] << 24);
    }

    public static ReadOnlySpan<byte> ReadSpan(ReadOnlySpan<byte> plainSettingsSpan, ref int position) {
        var length = ReadInt(plainSettingsSpan, ref position);
        var plainSpan = plainSettingsSpan.Slice(position, length);

        position += length;

        return plainSpan;
    }

    public static string ReadString(ReadOnlySpan<byte> plainSettingsSpan, ref int position)
        => Encoding.UTF8.GetString(ReadSpan(plainSettingsSpan, ref position));

    public static SecureBuffer ReadSecureBuffer(ReadOnlySpan<byte> plainSettingsSpan, ref int position)
        => new SecureBuffer(ReadSpan(plainSettingsSpan, ref position).ToArray());

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
