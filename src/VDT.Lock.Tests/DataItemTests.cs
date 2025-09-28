using System;
using System.Collections.Generic;
using Xunit;

namespace VDT.Lock.Tests;

public class DataItemTests {
    [Fact]
    public void DeserializeFrom() {
        var plainSpan = new ReadOnlySpan<byte>([.. DataProvider.CreateSerializedIdentity(0), 4, 0, 0, 0, 110, 97, 109, 101, 61, 0, 0, 0, .. DataProvider.CreateSerializedField(1, [110, 97, 109, 101], [118, 97, 108, 117, 101]), 49, 0, 0, 0, .. DataProvider.CreateSerializedValue(2, [108, 97, 98, 101, 108]), 52, 0, 0, 0, .. DataProvider.CreateSerializedValue(3, [108, 111, 99, 97, 116, 105, 111, 110])]);

        using var subject = DataItem.DeserializeFrom(plainSpan);

        Assert.Equal(DataProvider.CreateIdentity(0), subject.Identity);
        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), subject.Name);
        Assert.Equal(new ReadOnlySpan<byte>([118, 97, 108, 117, 101]), Assert.Single(subject.Fields).Value);
        Assert.Equal(new ReadOnlySpan<byte>([108, 97, 98, 101, 108]), Assert.Single(subject.Labels).Value);
        Assert.Equal(new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]), Assert.Single(subject.Locations).Value);
    }

    [Fact]
    public void Merge() {
        var expectedField = new DataField(DataProvider.CreateIdentity(1, 10), [118, 97, 108, 117, 101], [118, 97, 108, 117, 101]);
        var expectedLabel = new DataValue(DataProvider.CreateIdentity(2, 10), [108, 97, 98, 101, 108]);
        var expectedLocation = new DataValue(DataProvider.CreateIdentity(3, 10), [108, 111, 99, 97, 116, 105, 111, 110]);

        var expectedResult = new DataItem(DataProvider.CreateIdentity(0, 5), [110, 97, 109, 101]) {
            Fields = {
                new(DataProvider.CreateIdentity(1, 5), [111, 108, 100, 101, 114], [111, 108, 100, 101, 114]),
            },
            Labels = {
                new(DataProvider.CreateIdentity(2, 5), [111, 108, 100, 101, 114])
            },
            Locations = {
                new(DataProvider.CreateIdentity(3, 5), [111, 108, 100, 101, 114])
            }
        };

        var candidates = new List<DataItem>() {
            new(DataProvider.CreateIdentity(0, 3), [111, 108, 100, 101, 114]) {
                Fields = {
                    new(DataProvider.CreateIdentity(1, 5), [111, 108, 100, 101, 114], [111, 108, 100, 101, 114])
                },
                Labels = {
                    new(DataProvider.CreateIdentity(2, 5), [111, 108, 100, 101, 114])
                },
                Locations = {
                    new(DataProvider.CreateIdentity(3, 5), [111, 108, 100, 101, 114])
                }
            },
            expectedResult,
            new(DataProvider.CreateIdentity(0, 4), [111, 108, 100, 101, 114]) {
                Fields = {
                    expectedField
                },
                Labels = {
                    expectedLabel
                },
                Locations = {
                    expectedLocation
                }
            }
        };

        var result = DataItem.Merge(candidates);

        Assert.Same(expectedResult, result);
        Assert.Equal(expectedField, Assert.Single(result.Fields));
        Assert.Equal(expectedLabel, Assert.Single(result.Labels));
        Assert.Equal(expectedLocation, Assert.Single(result.Locations));

        foreach (var candidate in candidates) {
            Assert.Equal(candidate != expectedResult, candidate.IsDisposed);
        }
    }

    [Fact]
    public void Constructor() {
        var identity = new DataIdentity();
        var plainNameSpan = new ReadOnlySpan<byte>([110, 97, 109, 101]);
        using var subject = new DataItem(identity, plainNameSpan);

        Assert.Same(identity, subject.Identity);
        Assert.Equal(plainNameSpan, subject.Name);
    }

    [Fact]
    public void SetName() {
        using var subject = new DataItem();

        var plainPreviousValueBuffer = subject.GetBuffer("plainNameBuffer");

        subject.Name = new ReadOnlySpan<byte>([110, 97, 109, 101]);

        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), subject.Name);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }

    [Fact]
    public void SetFields() {
        using var subject = new DataItem();

        var previousFields = subject.Fields;
        var newFields = new DataCollection<DataField>();

        subject.Fields = newFields;

        Assert.Same(newFields, subject.Fields);
        Assert.True(previousFields.IsDisposed);
    }

    [Fact]
    public void SetLabels() {
        using var subject = new DataItem();

        var previousLabels = subject.Labels;
        var newLabels = new DataCollection<DataValue>();

        subject.Labels = newLabels;

        Assert.Same(newLabels, subject.Labels);
        Assert.True(previousLabels.IsDisposed);
    }

    [Fact]
    public void SetLocations() {
        using var subject = new DataItem();

        var previousLocations = subject.Locations;
        var newLocations = new DataCollection<DataValue>();

        subject.Locations = newLocations;

        Assert.Same(newLocations, subject.Locations);
        Assert.True(previousLocations.IsDisposed);
    }

    [Fact]
    public void FieldLengths() {
        using var subject = new DataItem([110, 97, 109, 101]);
        subject.Fields.Add(DataProvider.CreateField(0, [110, 97, 109, 101], [118, 97, 108, 117, 101]));
        subject.Labels.Add(DataProvider.CreateValue(1, [108, 97, 98, 101, 108]));
        subject.Locations.Add(DataProvider.CreateValue(2, [108, 111, 99, 97, 116, 105, 111, 110]));

        Assert.Equal([32, 4, 61, 49, 52], subject.FieldLengths);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new DataItem(DataProvider.CreateIdentity(0), [110, 97, 109, 101]);
        subject.Fields.Add(DataProvider.CreateField(1, [110, 97, 109, 101], [118, 97, 108, 117, 101]));
        subject.Labels.Add(DataProvider.CreateValue(2, [108, 97, 98, 101, 108]));
        subject.Locations.Add(DataProvider.CreateValue(3, [108, 111, 99, 97, 116, 105, 111, 110]));

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([218, 0, 0, 0, .. DataProvider.CreateSerializedIdentity(0), 4, 0, 0, 0, 110, 97, 109, 101, 61, 0, 0, 0, .. DataProvider.CreateSerializedField(1, [110, 97, 109, 101], [118, 97, 108, 117, 101]), 49, 0, 0, 0, .. DataProvider.CreateSerializedValue(2, [108, 97, 98, 101, 108]), 52, 0, 0, 0, .. DataProvider.CreateSerializedValue(3, [108, 111, 99, 97, 116, 105, 111, 110])]), result.GetValue());
    }

    [Fact]
    public void Dispose() {
        DataIdentity identity;
        SecureBuffer plainNameBuffer;
        DataCollection<DataField> fields;
        DataCollection<DataValue> labels;
        DataCollection<DataValue> locations;

        using (var subject = new DataItem()) {
            identity = subject.Identity;
            plainNameBuffer = subject.GetBuffer("plainNameBuffer");
            fields = subject.Fields;
            labels = subject.Labels;
            locations = subject.Locations;
        }

        Assert.True(identity.IsDisposed);
        Assert.True(plainNameBuffer.IsDisposed);
        Assert.True(fields.IsDisposed);
        Assert.True(labels.IsDisposed);
        Assert.True(locations.IsDisposed);
    }

    [Fact]
    public void IdentityThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Identity; });
    }

    [Fact]
    public void GetNameThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Name; });
    }

    [Fact]
    public void SetNameThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Name = new ReadOnlySpan<byte>([110, 97, 109, 101]));
    }

    [Fact]
    public void GetFieldsThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Fields);
    }

    [Fact]
    public void SetFieldsThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Fields = []);
    }

    [Fact]
    public void GetLabelsThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Labels);
    }

    [Fact]
    public void SetLabelsThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Labels = []);
    }

    [Fact]
    public void GetLocationsThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Locations);
    }

    [Fact]
    public void SetLocationsThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Locations = []);
    }

    [Fact]
    public void FieldLengthsThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.FieldLengths);
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        DataItem disposedSubject;
        using var plainBytes = new SecureByteList();

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.SerializeTo(plainBytes));
    }
}
