using System;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests.StorageSites;

public class FileSystemStorageSiteTests {
    [Fact]
    public void Constructor() {
        using var subject = new FileSystemStorageSite(new ReadOnlySpan<byte>([97, 98, 99]));

        Assert.Equal(new byte[] { 97, 98, 99 }, subject.Location);
    }
}
