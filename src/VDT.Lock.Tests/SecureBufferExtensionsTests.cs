using System;
using Xunit;

namespace VDT.Lock.Tests;

public class SecureBufferExtensionsTests {
    [Fact]
    public void ReadInt() {
        var subject = new SecureBuffer([0, 0, 0, 0, 8, 4, 2, 1, 0, 0, 0, 0]);
        var position = 4;

        var result = subject.ReadInt(ref position);

        Assert.Equal(8, position);
        Assert.Equal(16909320, result);
    }

    [Fact]
    public void ReadSpan() {
        var subject = new SecureBuffer([0, 0, 0, 0, 3, 0, 0, 0, 97, 98, 99, 0]);
        var position = 4;

        var result = subject.ReadSpan(ref position);

        Assert.Equal(11, position);
        Assert.Equal(new ReadOnlySpan<byte>([97, 98, 99]), result);
    }

    [Fact]
    public void ReadSecureBuffer() {
        var subject = new SecureBuffer([0, 0, 0, 0, 3, 0, 0, 0, 97, 98, 99, 0]);
        var position = 4;

        using var result = subject.ReadSecureBuffer(ref position);

        Assert.Equal(11, position);
        Assert.Equal([97, 98, 99], result.Value);
    }
}
