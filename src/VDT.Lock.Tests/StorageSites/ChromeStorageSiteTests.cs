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
        var plainNameSpan = new ReadOnlySpan<byte>([110, 97, 109, 101]);

        using var subject = new ChromeStorageSite(plainNameSpan);

        Assert.Equal(plainNameSpan, subject.Name);
    }

    [Fact]
    public void FieldLengths() {
        using var subject = new ChromeStorageSite([110, 97, 109, 101]);

        Assert.Equal([0, 4], subject.FieldLengths);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new ChromeStorageSite([110, 97, 109, 101]);

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([12, 0, 0, 0, ChromeStorageSite.TypeId, 0, 0, 0, 4, 0, 0, 0, 110, 97, 109, 101]), result.GetValue());
    }

    [Fact]
    public void FieldLengthsThrowsIfDisposed() {
        ChromeStorageSite subject;

        using (subject = new([])) { }

        Assert.Throws<ObjectDisposedException>(() => subject.FieldLengths);
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        ChromeStorageSite subject;
        using var plainBytes = new SecureByteList();

        using (subject = new([])) { }

        Assert.Throws<ObjectDisposedException>(() => subject.SerializeTo(plainBytes));
    }
}
