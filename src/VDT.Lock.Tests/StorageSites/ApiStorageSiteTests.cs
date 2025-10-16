using System;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests.StorageSites;

public class ApiStorageSiteTests {
    [Fact]
    public void DeserializeFrom() {
        var result = ApiStorageSite.DeserializeFrom(new ReadOnlySpan<byte>([4, 0, 0, 0, 110, 97, 109, 101, 8, 0, 0, 0, 108, 111, 99, 97, 116, 105, 111, 110, 2, 0, 0, 0, 105, 100, 6, 0, 0, 0, 115, 101, 99, 114, 101, 116]));

        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), result.Name);
        Assert.Equal(new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]), result.Location);
        Assert.Equal(new ReadOnlySpan<byte>([105, 100]), result.GetBuffer("plainDataStoreIdBuffer"));
        Assert.Equal(new ReadOnlySpan<byte>([115, 101, 99, 114, 101, 116]), result.GetBuffer("plainSecretBuffer"));
    }

    [Fact]
    public void SetLocation() {
        var subject = new ApiStorageSite([], []);
        var plainPreviousValueBuffer = subject.GetBuffer("plainLocationBuffer");

        subject.Location = new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]);

        Assert.Equal(new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]), subject.Location);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }

    [Fact]
    public void Constructor() {
        var plainNameSpan = new ReadOnlySpan<byte>([110, 97, 109, 101]);
        var plainLocationSpan = new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]);
        var plainDataStoreIdSpan = new ReadOnlySpan<byte>([105, 100]);
        var plainSecretSpan = new ReadOnlySpan<byte>([115, 101, 99, 114, 101, 116]);

        using var subject = new ApiStorageSite(plainNameSpan, plainLocationSpan, plainDataStoreIdSpan, plainSecretSpan);

        Assert.Equal(plainNameSpan, subject.Name);
        Assert.Equal(plainLocationSpan, subject.Location);
        Assert.Equal(plainDataStoreIdSpan, subject.GetBuffer("plainDataStoreIdBuffer"));
        Assert.Equal(plainSecretSpan, subject.GetBuffer("plainSecretBuffer"));
    }

    [Fact]
    public void FieldLengths() {
        using var subject = new ApiStorageSite([110, 97, 109, 101], [108, 111, 99, 97, 116, 105, 111, 110], [105, 100], [115, 101, 99, 114, 101, 116]);

        Assert.Equal([0, 4, 8, 2, 6], subject.FieldLengths);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new ApiStorageSite([110, 97, 109, 101], [108, 111, 99, 97, 116, 105, 111, 110], [105, 100], [115, 101, 99, 114, 101, 116]);

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([40, 0, 0, 0, ApiStorageSite.TypeId, 0, 0, 0, 4, 0, 0, 0, 110, 97, 109, 101, 8, 0, 0, 0, 108, 111, 99, 97, 116, 105, 111, 110, 2, 0, 0, 0, 105, 100, 6, 0, 0, 0, 115, 101, 99, 114, 101, 116]), result.GetValue());
    }

    [Fact]
    public void GetLocationThrowsIfDisposed() {
        ApiStorageSite subject;

        using (subject = new([], [])) { }

        Assert.Throws<ObjectDisposedException>(() => { var _ = subject.Location; });
    }

    [Fact]
    public void SetLocationThrowsIfDisposed() {
        ApiStorageSite subject;

        using (subject = new([], [])) { }

        Assert.Throws<ObjectDisposedException>(() => subject.Location = new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]));
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        ApiStorageSite subject;
        using var plainBytes = new SecureByteList();

        using (subject = new([], [])) { }

        Assert.Throws<ObjectDisposedException>(() => subject.SerializeTo(plainBytes));
    }
}
