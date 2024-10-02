using System;
using Xunit;

namespace VDT.Lock.Tests;

public class DataItemTests {
    [Fact]
    public void Constructor() {
        using var plainNameBuffer = new SecureBuffer([98, 97, 114]);

        using var subject = new DataItem(plainNameBuffer);

        Assert.Equal(plainNameBuffer.Value, subject.Name);
    }

    [Fact]
    public void SetName() {
        using var plainNameBuffer = new SecureBuffer([98, 97, 114]);

        using var subject = new DataItem(plainNameBuffer);

        var plainPreviousValueBuffer = subject.GetBuffer("plainNameBuffer");

        subject.Name = new ReadOnlySpan<byte>([99, 99, 99]);

        Assert.Equal(new ReadOnlySpan<byte>([99, 99, 99]), subject.Name);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }
}
