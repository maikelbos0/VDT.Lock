using System;
using Xunit;

namespace VDT.Lock.Tests;

public class DataItemTests {
    [Fact]
    public void Constructor() {
        using var subject = new DataItem([98, 97, 114]);

        Assert.Equal(new ReadOnlySpan<byte>([98, 97, 114]), subject.Name);
    }

    [Fact]
    public void SetName() {
        using var subject = new DataItem([98, 97, 114]);

        var plainPreviousValueBuffer = subject.GetBuffer("plainNameBuffer");

        subject.Name = new ReadOnlySpan<byte>([99, 99, 99]);

        Assert.Equal(new ReadOnlySpan<byte>([99, 99, 99]), subject.Name);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }
}
