using System;
using Xunit;

namespace VDT.Lock.Tests;

public class SettingsSerializerTests {
    [Fact]
    public void ReadInt() {
        var plainSettingsSpan = new ReadOnlySpan<byte>([0, 0, 0, 0, 8, 4, 2, 1, 0, 0, 0, 0]);
        var position = 4;

        var result = SettingsSerializer.ReadInt(plainSettingsSpan, ref position);

        Assert.Equal(8, position);
        Assert.Equal(16909320, result);
    }

    [Fact]
    public void ReadString() {
        var plainSettingsSpan = new ReadOnlySpan<byte>([0, 0, 0, 0, 3, 0, 0, 0, 97, 98, 99, 0]);
        var position = 4;

        var result = SettingsSerializer.ReadString(plainSettingsSpan, ref position);

        Assert.Equal(11, position);
        Assert.Equal("abc", result);
    }

    [Fact]
    public void ReadSecureBuffer() {
        var plainSettingsSpan = new ReadOnlySpan<byte>([0, 0, 0, 0, 3, 0, 0, 0, 97, 98, 99, 0]);
        var position = 4;

        using var result = SettingsSerializer.ReadSecureBuffer(plainSettingsSpan, ref position);

        Assert.Equal(11, position);
        Assert.Equal(new byte[] { 97, 98, 99 }, result.Value);
    }
}
