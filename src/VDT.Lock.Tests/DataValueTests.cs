using System;
using System.Collections.Generic;
using Xunit;

namespace VDT.Lock.Tests;

public class DataValueTests {
    [Fact]
    public void DeserializeFrom() {
        var plainSpan = new ReadOnlySpan<byte>([.. DataProvider.CreateSerializedIdentity(0), 5, 0, 0, 0, 118, 97, 108, 117, 101]);

        using var subject = DataValue.DeserializeFrom(plainSpan);

        Assert.Equal(DataProvider.CreateIdentity(0), subject.Identity);
        Assert.Equal(new ReadOnlySpan<byte>([118, 97, 108, 117, 101]), subject.Value);
    }

    [Fact]
    public void SelectNewest() {
        var expectedResult = new DataValue(DataProvider.CreateIdentity(0, 5), [118, 97, 108, 117, 101]);

        var candidates = new List<DataValue>() {
            new(DataProvider.CreateIdentity(0, 3), [111, 108, 100, 101, 114]),
            expectedResult,
            new(DataProvider.CreateIdentity(0, 4), [111, 108, 100, 101, 114])
        };

        var result = DataValue.Merge(candidates);

        Assert.Same(expectedResult, result);

        foreach (var candidate in candidates) {
            Assert.Equal(candidate != expectedResult, candidate.IsDisposed);
        }
    }

    [Fact]
    public void Constructor() {
        var identity = new DataIdentity();
        var plainValueSpan = new ReadOnlySpan<byte>([118, 97, 108, 117, 101]);

        using var subject = new DataValue(identity, plainValueSpan);

        Assert.Same(identity, subject.Identity);
        Assert.Equal(plainValueSpan, subject.Value);
    }

    [Fact]
    public void SetValue() {
        using var subject = new DataValue(DataProvider.CreateIdentity(0, 0), []);

        var previousVersion = subject.Identity.Version;
        var plainPreviousValueBuffer = subject.GetBuffer("plainValueBuffer");

        subject.Value = new ReadOnlySpan<byte>([118, 97, 108, 117, 101]);

        Assert.Equal(new ReadOnlySpan<byte>([118, 97, 108, 117, 101]), subject.Value);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
        Assert.False(previousVersion.SequenceEqual(subject.Identity.Version));
    }

    [Fact]
    public void FieldLengths() {
        using var subject = new DataValue([118, 97, 108, 117, 101]);

        Assert.Equal([32, 5], subject.FieldLengths);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new DataValue(DataProvider.CreateIdentity(0), [118, 97, 108, 117, 101]);

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([45, 0, 0, 0, .. DataProvider.CreateSerializedIdentity(0), 5, 0, 0, 0, 118, 97, 108, 117, 101]), result.GetValue());
    }

    [Fact]
    public void Dispose() {
        DataValue subject;
        DataIdentity identity;
        SecureBuffer plainValueBuffer;

        using (subject = new()) {
            identity = subject.Identity;
            plainValueBuffer = subject.GetBuffer("plainValueBuffer");
        }

        Assert.True(subject.IsDisposed);
        Assert.True(identity.IsDisposed);
        Assert.True(plainValueBuffer.IsDisposed);
    }

    [Fact]
    public void IdentityThrowsIfDisposed() {
        DataValue disposedSubject;

        using (var subject = new DataValue()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Identity; });
    }

    [Fact]
    public void GetValueThrowsIfDisposed() {
        DataValue disposedSubject;

        using (var subject = new DataValue()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Value; });
    }

    [Fact]
    public void SetValueThrowsIfDisposed() {
        DataValue disposedSubject;

        using (var subject = new DataValue()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Value = new ReadOnlySpan<byte>([118, 97, 108, 117, 101]));
    }

    [Fact]
    public void FieldLengthsThrowsIfDisposed() {
        DataValue disposedSubject;

        using (var subject = new DataValue()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.FieldLengths);
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        DataValue disposedSubject;
        using var plainBytes = new SecureByteList();

        using (var subject = new DataValue()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.SerializeTo(plainBytes));
    }
}
