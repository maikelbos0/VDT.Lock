using NSubstitute;
using System;
using Xunit;

namespace VDT.Lock.Tests;

public class DataIdentityTests {
    [Fact]
    public void EqualityOperatorIsTrueForNulls() {
        DataIdentity? a = null;
        DataIdentity? b = null;

        Assert.True(a == b);
    }

    [Fact]
    public void EqualityOperatorIsFalseWhenAIsNull() {
        DataIdentity? a = null;
        var b = new DataIdentity();

        Assert.False(a == b);
    }

    [Fact]
    public void EqualityOperatorIsFalseWhenBIsNull() {
        var a = new DataIdentity();
        DataIdentity? b = null;

        Assert.False(a == b);
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
    public void EqualityOperatorUsesOnlyKey(byte[] plainSpanA, byte[] plainSpanB, bool expectedResult) {
        using var a = DataIdentity.DeserializeFrom(plainSpanA);
        using var b = DataIdentity.DeserializeFrom(plainSpanB);

        Assert.Equal(expectedResult, a == b);
    }

    [Theory]
    [InlineData(
        new byte[] { 16, 0, 0, 0, 56, 240, 157, 219, 241, 61, 91, 71, 186, 251, 45, 225, 99, 172, 214, 4, 8, 0, 0, 0, 226, 189, 189, 101, 0, 0, 0, 0 },
        new byte[] { 16, 0, 0, 0, 56, 240, 157, 219, 241, 61, 91, 71, 186, 251, 45, 225, 99, 172, 214, 4, 8, 0, 0, 0, 225, 188, 188, 99, 0, 0, 0, 0 },
        false
    )]
    [InlineData(
        new byte[] { 16, 0, 0, 0, 56, 240, 157, 219, 241, 61, 91, 71, 186, 251, 45, 225, 99, 172, 214, 4, 8, 0, 0, 0, 226, 189, 189, 101, 0, 0, 0, 0 },
        new byte[] { 16, 0, 0, 0, 56, 240, 157, 219, 241, 61, 91, 71, 186, 251, 45, 225, 99, 172, 214, 3, 8, 0, 0, 0, 226, 189, 189, 101, 0, 0, 0, 0 },
        true
    )]
    public void InequalityOperator(byte[] plainSpanA, byte[] plainSpanB, bool expectedResult) {
        using var a = DataIdentity.DeserializeFrom(plainSpanA);
        using var b = DataIdentity.DeserializeFrom(plainSpanB);

        Assert.Equal(expectedResult, a != b);
    }

    [Fact]
    public void SelectNewest() {
        var key = Guid.NewGuid().ToByteArray();

        using var identity1 = DataIdentity.DeserializeFrom([16, 0, 0, 0, .. key, 8, 0, 0, 0, 226, 189, 209, 101, 0, 0, 0, 0]);
        var candidate1 = Substitute.For<IIdentifiableData>();
        candidate1.Identity.Returns(identity1);

        using var identity2 = DataIdentity.DeserializeFrom([16, 0, 0, 0, .. key, 8, 0, 0, 0, 226, 189, 189, 201, 0, 0, 0, 0]);
        var candidate2 = Substitute.For<IIdentifiableData>();
        candidate2.Identity.Returns(identity2);

        using var identity3 = DataIdentity.DeserializeFrom([16, 0, 0, 0, .. key, 8, 0, 0, 0, 226, 209, 189, 101, 0, 0, 0, 0]);
        var candidate3 = Substitute.For<IIdentifiableData>();
        candidate3.Identity.Returns(identity3);

        using var identity4 = DataIdentity.DeserializeFrom([16, 0, 0, 0, .. key, 8, 0, 0, 0, 246, 189, 209, 101, 0, 0, 0, 0]);
        var candidate4 = Substitute.For<IIdentifiableData>();
        candidate4.Identity.Returns(identity4);

        var result = DataIdentity.SelectNewest([candidate1, candidate2, candidate3, candidate4]);

        Assert.Same(candidate2, result);
    }

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
        var previousVersion = subject.Version[0]
            | ((long)subject.Version[1] << 8)
            | ((long)subject.Version[2] << 16)
            | ((long)subject.Version[3] << 24)
            | ((long)subject.Version[4] << 32)
            | ((long)subject.Version[5] << 40)
            | ((long)subject.Version[6] << 48)
            | ((long)subject.Version[7] << 56);

        subject.Update();

        var version = subject.Version[0]
            | ((long)subject.Version[1] << 8)
            | ((long)subject.Version[2] << 16)
            | ((long)subject.Version[3] << 24)
            | ((long)subject.Version[4] << 32)
            | ((long)subject.Version[5] << 40)
            | ((long)subject.Version[6] << 48)
            | ((long)subject.Version[7] << 56);

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
        using IDisposable other = DataIdentity.DeserializeFrom(otherPlainSpan);

        Assert.Equal(expectedResult, subject.Equals(other));
    }

    [Fact]
    public void EqualsIsFalseForNull() {
        using var subject = new DataIdentity();

        Assert.False(subject.Equals(null));
    }

    [Fact]
    public void Dispose() {
        DataIdentity subject;
        SecureBuffer plainKeyBuffer;
        SecureBuffer plainVersionBuffer;

        using (subject = new()) {
            plainKeyBuffer = subject.GetBuffer("plainKeyBuffer");
            plainVersionBuffer = subject.GetBuffer("plainVersionBuffer");
        }

        Assert.True(subject.IsDisposed);
        Assert.True(plainKeyBuffer.IsDisposed);
        Assert.True(plainVersionBuffer.IsDisposed);
    }

    [Fact]
    public void KeyThrowsIfDisposed() {
        DataIdentity subject;

        using (subject = new()) { }

        Assert.Throws<ObjectDisposedException>(() => { var _ = subject.Key; });
    }

    [Fact]
    public void VersionThrowsIfDisposed() {
        DataIdentity subject;

        using (subject = new()) { }

        Assert.Throws<ObjectDisposedException>(() => { var _ = subject.Version; });
    }

    [Fact]
    public void FieldLengthsThrowsIfDisposed() {
        DataIdentity subject;

        using (subject = new()) { }

        Assert.Throws<ObjectDisposedException>(() => subject.FieldLengths);
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        DataIdentity subject;
        using var plainBytes = new SecureByteList();

        using (subject = new()) { }

        Assert.Throws<ObjectDisposedException>(() => subject.SerializeTo(plainBytes));
    }
}
