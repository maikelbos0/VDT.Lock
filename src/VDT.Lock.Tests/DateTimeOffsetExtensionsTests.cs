using System;
using Xunit;

namespace VDT.Lock.Tests;

public class DateTimeOffsetExtensionsTests {
    [Fact]
    public void ToVersion() {
        var subject = new DateTimeOffset(2024, 2, 3, 4, 15, 30, TimeSpan.Zero);

        var result = subject.ToVersion();

        Assert.Equal([226, 189, 189, 101, 0, 0, 0, 0], result);
    }
}
