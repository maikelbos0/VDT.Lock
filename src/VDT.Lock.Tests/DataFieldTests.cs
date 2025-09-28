using System;
using System.Collections.Generic;
using Xunit;

namespace VDT.Lock.Tests;

public class DataFieldTests {
    [Fact]
    public void DeserializeFrom() {

        var plainSpan = new ReadOnlySpan<byte>([.. DataProvider.CreateSerializedIdentity(0), 4, 0, 0, 0, 110, 97, 109, 101, 5, 0, 0, 0, 118, 97, 108, 117, 101, 52, 0, 0, 0, .. DataProvider.CreateSerializedValue(1, [115, 101, 108, 101, 99, 116, 111, 114])]);

        using var subject = DataField.DeserializeFrom(plainSpan);

        Assert.Equal(DataProvider.CreateIdentity(0), subject.Identity);
        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), subject.Name);
        Assert.Equal(new ReadOnlySpan<byte>([118, 97, 108, 117, 101]), subject.Value);
        Assert.Equal(new ReadOnlySpan<byte>([115, 101, 108, 101, 99, 116, 111, 114]), Assert.Single(subject.Selectors).Value);
    }

    [Fact]
    public void Merge() {
        var expectedSelector = new DataValue(DataProvider.CreateIdentity(1, 10), [115, 101, 108, 101, 99, 116, 111, 114]);
        var expectedResult = new DataField(DataProvider.CreateIdentity(0, 5), [110, 97, 109, 101], [118, 97, 108, 117, 101]) {
            Selectors = {
                new(DataProvider.CreateIdentity(1, 5), [111, 108, 100, 101, 114]),
            }
        };

        var candidates = new List<DataField>() {
            new(DataProvider.CreateIdentity(0, 3), [111, 108, 100, 101, 114], [111, 108, 100, 101, 114]) {
                Selectors = {
                    new(DataProvider.CreateIdentity(1, 5), [111, 108, 100, 101, 114])
                }
            },
            expectedResult,
            new(DataProvider.CreateIdentity(0, 4), [111, 108, 100, 101, 114], [111, 108, 100, 101, 114]) {
                Selectors = {
                    expectedSelector
                }
            }
        };

        var result = DataField.Merge(candidates);

        Assert.Same(expectedResult, result);
        Assert.Equal(expectedSelector, Assert.Single(result.Selectors));

        foreach (var candidate in candidates) {
            Assert.Equal(candidate != expectedResult, candidate.IsDisposed);
        }
    }

    [Fact]
    public void Constructor() {
        var identity = new DataIdentity();
        var plainNameSpan = new ReadOnlySpan<byte>([110, 97, 109, 101]);
        var plainDataSpan = new ReadOnlySpan<byte>([118, 97, 108, 117, 101]);

        using var subject = new DataField(identity, plainNameSpan, plainDataSpan);

        Assert.Same(identity, subject.Identity);
        Assert.Equal(plainNameSpan, subject.Name);
        Assert.Equal(plainDataSpan, subject.Value);
    }

    [Fact]
    public void SetName() {
        using var subject = new DataField();

        var plainPreviousValueBuffer = subject.GetBuffer("plainNameBuffer");

        subject.Name = new ReadOnlySpan<byte>([110, 97, 109, 101]);

        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), subject.Name);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }

    [Fact]
    public void SetValue() {
        using var subject = new DataField();

        var plainPreviousValueBuffer = subject.GetBuffer("plainValueBuffer");

        subject.Value = new ReadOnlySpan<byte>([118, 97, 108, 117, 101]);

        Assert.Equal(new ReadOnlySpan<byte>([118, 97, 108, 117, 101]), subject.Value);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }

    [Fact]
    public void SetSelectors() {
        using var subject = new DataField();

        var previousFields = subject.Selectors;
        var newFields = new DataCollection<DataValue>();

        subject.Selectors = newFields;

        Assert.Same(newFields, subject.Selectors);
        Assert.True(previousFields.IsDisposed);
    }

    [Fact]
    public void FieldLengths() {
        using var subject = new DataField([110, 97, 109, 101], [118, 97, 108, 117, 101]) {
            Selectors = {
                new DataValue([105, 116, 101, 109])
            }
        };

        Assert.Equal([32, 4, 5, 48], subject.FieldLengths);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new DataField(DataProvider.CreateIdentity(0), [110, 97, 109, 101], [118, 97, 108, 117, 101]) {
            Selectors = {
                DataProvider.CreateValue(1, [105, 116, 101, 109])
            }
        };
        
        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([105, 0, 0, 0, .. DataProvider.CreateSerializedIdentity(0), 4, 0, 0, 0, 110, 97, 109, 101, 5, 0, 0, 0, 118, 97, 108, 117, 101, 48, 0, 0, 0, .. DataProvider.CreateSerializedValue(1, [105, 116, 101, 109])]), result.GetValue());
    }

    [Fact]
    public void Dispose() {
        DataIdentity identity;
        SecureBuffer plainNameBuffer;
        SecureBuffer plainDataBuffer;
        DataCollection<DataValue> selectors;

        using (var subject = new DataField()) {
            identity = subject.Identity;
            plainNameBuffer = subject.GetBuffer("plainNameBuffer");
            plainDataBuffer = subject.GetBuffer("plainValueBuffer");
            selectors = subject.Selectors;
        }

        Assert.True(identity.IsDisposed);
        Assert.True(plainNameBuffer.IsDisposed);
        Assert.True(plainDataBuffer.IsDisposed);
        Assert.True(selectors.IsDisposed);
    }

    [Fact]
    public void IsDisposed() {
        DataField disposedSubject;

        using (var subject = new DataField()) {
            disposedSubject = subject;
        }

        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void IdentityThrowsIfDisposed() {
        DataField disposedSubject;

        using (var subject = new DataField()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Identity; });
    }

    [Fact]
    public void GetNameThrowsIfDisposed() {
        DataField disposedSubject;

        using (var subject = new DataField()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Name; });
    }

    [Fact]
    public void SetNameThrowsIfDisposed() {
        DataField disposedSubject;

        using (var subject = new DataField()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Name = new ReadOnlySpan<byte>([110, 97, 109, 101]));
    }

    [Fact]
    public void GetValueThrowsIfDisposed() {
        DataField disposedSubject;

        using (var subject = new DataField()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Value; });
    }

    [Fact]
    public void SetValueThrowsIfDisposed() {
        DataField disposedSubject;

        using (var subject = new DataField()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Value = new ReadOnlySpan<byte>([118, 97, 108, 117, 101]));
    }

    [Fact]
    public void GetSelectorsThrowsIfDisposed() {
        DataField disposedSubject;

        using (var subject = new DataField()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Selectors);
    }

    [Fact]
    public void SetSelectorsThrowsIfDisposed() {
        DataField disposedSubject;

        using (var subject = new DataField()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Selectors = []);
    }

    [Fact]
    public void FieldLengthsThrowsIfDisposed() {
        DataField disposedSubject;

        using (var subject = new DataField()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.FieldLengths);
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        DataField disposedSubject;
        using var plainBytes = new SecureByteList();

        using (var subject = new DataField()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.SerializeTo(plainBytes));
    }
}
