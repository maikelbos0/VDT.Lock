using System;
using Xunit;

namespace VDT.Lock.Tests;

public class ReadOnlySpanExtensionsTests {
    [Fact]
    public void ReadInt() {
        var subject = new ReadOnlySpan<byte>([0, 0, 0, 0, 8, 4, 2, 1, 0, 0, 0, 0]);
        var position = 4;

        var result = subject.ReadInt(ref position);

        Assert.Equal(8, position);
        Assert.Equal(16909320, result);
    }

    [Fact]
    public void ReadString() {
        var subject = new ReadOnlySpan<byte>([0, 0, 0, 0, 3, 0, 0, 0, 97, 98, 99, 0]);
        var position = 4;

        var result = subject.ReadString(ref position);

        Assert.Equal(11, position);
        Assert.Equal("abc", result);
    }

    [Fact]
    public void ReadSecureBuffer() {
        var subject = new ReadOnlySpan<byte>([0, 0, 0, 0, 3, 0, 0, 0, 97, 98, 99, 0]);
        var position = 4;

        using var result = subject.ReadSecureBuffer(ref position);

        Assert.Equal(11, position);
        Assert.Equal(new byte[] { 97, 98, 99 }, result.Value);
    }
}
