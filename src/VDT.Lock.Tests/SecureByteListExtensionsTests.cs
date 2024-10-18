using System;
using Xunit;

namespace VDT.Lock.Tests;

public class SecureByteListExtensionsTests {
    [Fact]
    public void WriteInt() {
        using var subject = new SecureByteList();

        subject.WriteInt(16909320);

        Assert.Equal(new byte[] { 8, 4, 2, 1 }, subject.GetValue());
    }

    [Fact]
    public void WriteSpan() {
        using var subject = new SecureByteList();

        subject.WriteSpan(new ReadOnlySpan<byte>([97, 98, 99]));

        Assert.Equal(new byte[] { 3, 0, 0, 0, 97, 98, 99 }, subject.GetValue());
    }

    [Fact]
    public void WriteSecureBuffer() {
        using var subject = new SecureByteList();
        using var plainBuffer = new SecureBuffer([97, 98, 99]);

        subject.WriteSecureBuffer(plainBuffer);

        Assert.Equal(new byte[] { 3, 0, 0, 0, 97, 98, 99 }, subject.GetValue());
    }
}
