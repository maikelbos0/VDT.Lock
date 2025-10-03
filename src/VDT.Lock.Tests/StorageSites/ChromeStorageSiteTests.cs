using System;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests.StorageSites;

public class ChromeStorageSiteTests {
    [Fact]
    public void DeserializeFrom() {
        var result = ChromeStorageSite.DeserializeFrom(new ReadOnlySpan<byte>([4, 0, 0, 0, 110, 97, 109, 101]));

        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), result.Name);
    }

    [Fact]
    public void Constructor() {
        using var subject = new ChromeStorageSite(new ReadOnlySpan<byte>([110, 97, 109, 101]));

        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), subject.Name);
    }

    [Fact]
    public void FieldLengths() {
        using var subject = new ChromeStorageSite(new ReadOnlySpan<byte>([110, 97, 109, 101]), null!);

        Assert.Equal([0, 4], subject.FieldLengths);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new ChromeStorageSite(new ReadOnlySpan<byte>([110, 97, 109, 101]), null!);

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([12, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 110, 97, 109, 101]), result.GetValue());
    }
}
