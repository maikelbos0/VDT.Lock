using System;
using Xunit;

namespace VDT.Lock.Tests;

public class DataItemTests {
    [Fact]
    public void DeserializeFrom() {
        var plainSpan = new ReadOnlySpan<byte>([4, 0, 0, 0, 110, 97, 109, 101, 61, 0, 0, 0, .. DataProvider.CreateSerializedField(0, [110, 97, 109, 101], [118, 97, 108, 117, 101]), 49, 0, 0, 0, .. DataProvider.CreateSerializedValue(1, [108, 97, 98, 101, 108]), 52, 0, 0, 0, .. DataProvider.CreateSerializedValue(2, [108, 111, 99, 97, 116, 105, 111, 110])]);

        using var subject = DataItem.DeserializeFrom(plainSpan);

        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), subject.Name);
        Assert.Equal(new ReadOnlySpan<byte>([118, 97, 108, 117, 101]), Assert.Single(subject.Fields).Value);
        Assert.Equal(new ReadOnlySpan<byte>([108, 97, 98, 101, 108]), Assert.Single(subject.Labels).Value);
        Assert.Equal(new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]), Assert.Single(subject.Locations).Value);
    }

    [Fact]
    public void Constructor() {
        using var subject = new DataItem([110, 97, 109, 101]);

        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), subject.Name);
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

        Assert.Equal([4, 61, 49, 52], subject.FieldLengths);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new DataItem([110, 97, 109, 101]);
        subject.Fields.Add(DataProvider.CreateField(0, [110, 97, 109, 101], [118, 97, 108, 117, 101]));
        subject.Labels.Add(DataProvider.CreateValue(1, [108, 97, 98, 101, 108]));
        subject.Locations.Add(DataProvider.CreateValue(2, [108, 111, 99, 97, 116, 105, 111, 110]));

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([182, 0, 0, 0, 4, 0, 0, 0, 110, 97, 109, 101, 61, 0, 0, 0, .. DataProvider.CreateSerializedField(0, [110, 97, 109, 101], [118, 97, 108, 117, 101]), 49, 0, 0, 0, .. DataProvider.CreateSerializedValue(1, [108, 97, 98, 101, 108]), 52, 0, 0, 0, .. DataProvider.CreateSerializedValue(2, [108, 111, 99, 97, 116, 105, 111, 110])]), result.GetValue());
    }

    [Fact]
    public void Dispose() {
        SecureBuffer plainNameBuffer;
        DataCollection<DataField> fields;
        DataCollection<DataValue> labels;
        DataCollection<DataValue> locations;

        using (var subject = new DataItem()) {
            plainNameBuffer = subject.GetBuffer("plainNameBuffer");
            fields = subject.Fields;
            labels = subject.Labels;
            locations = subject.Locations;
        }

        Assert.True(plainNameBuffer.IsDisposed);
        Assert.True(fields.IsDisposed);
        Assert.True(labels.IsDisposed);
        Assert.True(locations.IsDisposed);
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
