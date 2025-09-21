using System;
using Xunit;

namespace VDT.Lock.Tests;

public class DataIdentityTests {
    [Fact]
    public void DeserializeFrom() {
        var plainSpan = new ReadOnlySpan<byte>([16, 0, 0, 0, 56, 240, 157, 219, 241, 61, 91, 71, 186, 251, 45, 225, 99, 172, 214, 4, 8, 0, 0, 0, 226, 189, 189, 101, 0, 0, 0, 0]);

        using var subject = DataIdentity.DeserializeFrom(plainSpan);

        Assert.Equal(new ReadOnlySpan<byte>([56, 240, 157, 219, 241, 61, 91, 71, 186, 251, 45, 225, 99, 172, 214, 4]), subject.Key);
        Assert.Equal(new ReadOnlySpan<byte>([226, 189, 189, 101, 0, 0, 0, 0]), subject.Version);
    }

    [Fact]
    public void EmptyConstructor() {
        using var subject = new DataIdentity();

        Assert.Equal(16, subject.Key.Length);
        Assert.Equal(8, subject.Version.Length);
    }

    [Fact]
    public void FieldLengths() {
        using var subject = new DataIdentity();

        Assert.Equal([16, 8], subject.FieldLengths);
    }

    [Fact]
    public void Update() {
        var plainSpan = new ReadOnlySpan<byte>([16, 0, 0, 0, 56, 240, 157, 219, 241, 61, 91, 71, 186, 251, 45, 225, 99, 172, 214, 4, 8, 0, 0, 0, 226, 189, 189, 101, 0, 0, 0, 0]);

        using var subject = DataIdentity.DeserializeFrom(plainSpan);
        var previousVersion = subject.Version[0] | (subject.Version[1] << 8) | (subject.Version[2] << 16) | (subject.Version[3] << 24) | (subject.Version[4] << 32) | (subject.Version[5] << 40) | (subject.Version[6] << 48) | (subject.Version[7] << 56);

        subject.Update();

        var version = subject.Version[0] | (subject.Version[1] << 8) | (subject.Version[2] << 16) | (subject.Version[3] << 24) | (subject.Version[4] << 32) | (subject.Version[5] << 40) | (subject.Version[6] << 48) | (subject.Version[7] << 56);

        Assert.True(previousVersion <= version);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new DataIdentity();

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([32, 0, 0, 0, 16, 0, 0, 0, .. subject.Key, 8, 0, 0, 0, .. subject.Version]), result.GetValue());
    }

    [Theory]
    [InlineData(new byte[] { 16, 0, 0, 0, 56, 240, 157, 219, 241, 61, 91, 71, 186, 251, 45, 225, 99, 172, 214, 4, 8, 0, 0, 0, 226, 189, 189, 101, 0, 0, 0, 0 }, -764201580)]
    [InlineData(new byte[] { 16, 0, 0, 0, 56, 240, 157, 219, 241, 61, 91, 71, 186, 251, 45, 225, 99, 172, 214, 4, 8, 0, 0, 0, 225, 188, 188, 99, 0, 0, 0, 0 }, -764201580)]
    public void GetHashCodeUsesOnlyKey(byte[] plainSpan, int expectedResult) {
        using var subject = DataIdentity.DeserializeFrom(plainSpan);

        var result = subject.GetHashCode();

        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(
        new byte[] { 16, 0, 0, 0, 56, 240, 157, 219, 241, 61, 91, 71, 186, 251, 45, 225, 99, 172, 214, 4, 8, 0, 0, 0, 226, 189, 189, 101, 0, 0, 0, 0 },
        new byte[] { 16, 0, 0, 0, 56, 240, 157, 219, 241, 61, 91, 71, 186, 251, 45, 225, 99, 172, 214, 4, 8, 0, 0, 0, 225, 188, 188, 99, 0, 0, 0, 0 },
        true
    )]
    [InlineData(
        new byte[] { 16, 0, 0, 0, 56, 240, 157, 219, 241, 61, 91, 71, 186, 251, 45, 225, 99, 172, 214, 4, 8, 0, 0, 0, 226, 189, 189, 101, 0, 0, 0, 0 },
        new byte[] { 16, 0, 0, 0, 56, 240, 157, 219, 241, 61, 91, 71, 186, 251, 45, 225, 99, 172, 214, 3, 8, 0, 0, 0, 226, 189, 189, 101, 0, 0, 0, 0 },
        false
    )]
    public void EqualsUsesOnlyKey(byte[] plainSpan, byte[] otherPlainSpan, bool expectedResult) {
        using var subject = DataIdentity.DeserializeFrom(plainSpan);
        using var other = DataIdentity.DeserializeFrom(otherPlainSpan);

        Assert.Equal(expectedResult, subject.Equals(other));
    }

    [Fact]
    public void EqualsIsFalseForNull() {
        using var subject = new DataIdentity();

        Assert.False(subject.Equals(null));
    }

    [Fact]
    public void Dispose() {
        SecureBuffer plainKeyBuffer;
        SecureBuffer plainVersionBuffer;

        using (var subject = new DataIdentity()) {
            plainKeyBuffer = subject.GetBuffer("plainKeyBuffer");
            plainVersionBuffer = subject.GetBuffer("plainVersionBuffer");
        }

        Assert.True(plainKeyBuffer.IsDisposed);
        Assert.True(plainVersionBuffer.IsDisposed);
    }

    [Fact]
    public void IsDisposed() {
        DataIdentity disposedSubject;

        using (var subject = new DataIdentity()) {
            disposedSubject = subject;
        }

        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void KeyThrowsIfDisposed() {
        DataIdentity disposedSubject;

        using (var subject = new DataIdentity()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Key; });
    }

    [Fact]
    public void VersionThrowsIfDisposed() {
        DataIdentity disposedSubject;

        using (var subject = new DataIdentity()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Version; });
    }

    [Fact]
    public void FieldLengthsThrowsIfDisposed() {
        DataIdentity disposedSubject;

        using (var subject = new DataIdentity()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.FieldLengths);
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        DataIdentity disposedSubject;
        using var plainBytes = new SecureByteList();

        using (var subject = new DataIdentity()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.SerializeTo(plainBytes));
    }
}
