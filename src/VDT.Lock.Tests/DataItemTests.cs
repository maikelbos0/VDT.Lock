using System;
using Xunit;

namespace VDT.Lock.Tests;

public class DataItemTests {
    [Fact]
    public void DeserializeFromDeserializesName() {
        var plainSpan = new ReadOnlySpan<byte>([3, 0, 0, 0, 98, 97, 114, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

        using var subject = DataItem.DeserializeFrom(plainSpan);

        Assert.Equal(new ReadOnlySpan<byte>([98, 97, 114]), subject.Name);
    }

    [Fact]
    public void DeserializeFromDeserializesFields() {
        var plainSpan = new ReadOnlySpan<byte>([0, 0, 0, 0, 48, 0, 0, 0, 20, 0, 0, 0, 3, 0, 0, 0, 98, 97, 114, 5, 0, 0, 0, 1, 2, 3, 4, 5, 0, 0, 0, 0, 20, 0, 0, 0, 3, 0, 0, 0, 102, 111, 111, 5, 0, 0, 0, 5, 6, 7, 8, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

        using var subject = DataItem.DeserializeFrom(plainSpan);

        Assert.Equal(2, subject.Fields.Count);
        Assert.Contains(subject.Fields, field => field.Name.SequenceEqual(new ReadOnlySpan<byte>([98, 97, 114])) && field.Value.SequenceEqual(new ReadOnlySpan<byte>([1, 2, 3, 4, 5])));
        Assert.Contains(subject.Fields, field => field.Name.SequenceEqual(new ReadOnlySpan<byte>([102, 111, 111])) && field.Value.SequenceEqual(new ReadOnlySpan<byte>([5, 6, 7, 8, 9])));
    }

    [Fact]
    public void DeserializeFromDeserializesLabels() {
        var plainSpan = new ReadOnlySpan<byte>([0, 0, 0, 0, 0, 0, 0, 0, 24, 0, 0, 0, 7, 0, 0, 0, 3, 0, 0, 0, 98, 97, 114, 9, 0, 0, 0, 5, 0, 0, 0, 1, 2, 3, 4, 5, 0, 0, 0, 0]);

        using var subject = DataItem.DeserializeFrom(plainSpan);

        Assert.Equal(2, subject.Labels.Count);
        Assert.Contains(subject.Labels, label => label.Value.SequenceEqual(new ReadOnlySpan<byte>([98, 97, 114])));
        Assert.Contains(subject.Labels, label => label.Value.SequenceEqual(new ReadOnlySpan<byte>([1, 2, 3, 4, 5])));
    }

    [Fact]
    public void DeserializeFromDeserializesLocations() {
        var plainSpan = new ReadOnlySpan<byte>([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 24, 0, 0, 0, 7, 0, 0, 0, 3, 0, 0, 0, 98, 97, 114, 9, 0, 0, 0, 5, 0, 0, 0, 1, 2, 3, 4, 5]);

        using var subject = DataItem.DeserializeFrom(plainSpan);

        Assert.Equal(2, subject.Locations.Count);
        Assert.Contains(subject.Locations, location => location.Value.SequenceEqual(new ReadOnlySpan<byte>([98, 97, 114])));
        Assert.Contains(subject.Locations, location => location.Value.SequenceEqual(new ReadOnlySpan<byte>([1, 2, 3, 4, 5])));
    }

    [Fact]
    public void Constructor() {
        using var subject = new DataItem([98, 97, 114]);

        Assert.Equal(new ReadOnlySpan<byte>([98, 97, 114]), subject.Name);
    }

    [Fact]
    public void SetName() {
        using var subject = new DataItem();

        var plainPreviousValueBuffer = subject.GetBuffer("plainNameBuffer");

        subject.Name = new ReadOnlySpan<byte>([99, 99, 99]);

        Assert.Equal(new ReadOnlySpan<byte>([99, 99, 99]), subject.Name);
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
        using var subject = new DataItem([98, 97, 114]);
        subject.Fields.Add(new([102, 111, 111], [1, 2, 3, 4, 5]));
        subject.Fields.Add(new([98, 97, 114], [5, 6, 7, 8, 9]));
        subject.Labels.Add(new([102, 111, 111]));
        subject.Labels.Add(new([98, 97, 114]));
        subject.Locations.Add(new([102, 111, 111]));
        subject.Locations.Add(new([98, 97, 114]));

        Assert.Equal([3, 48, 22, 22], subject.FieldLengths);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new DataItem([98, 97, 114]);
        subject.Fields.Add(new([102, 111, 111], [1, 2, 3, 4, 5]));
        subject.Fields.Add(new([98, 97, 114], [5, 6, 7, 8, 9]));
        subject.Labels.Add(new([102, 111, 111]));
        subject.Labels.Add(new([98, 97, 114]));
        subject.Locations.Add(new([102, 111, 111]));
        subject.Locations.Add(new([98, 97, 114]));

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([111, 0, 0, 0, 3, 0, 0, 0, 98, 97, 114, 48, 0, 0, 0, 20, 0, 0, 0, 3, 0, 0, 0, 102, 111, 111, 5, 0, 0, 0, 1, 2, 3, 4, 5, 0, 0, 0, 0, 20, 0, 0, 0, 3, 0, 0, 0, 98, 97, 114, 5, 0, 0, 0, 5, 6, 7, 8, 9, 0, 0, 0, 0, 22, 0, 0, 0, 7, 0, 0, 0, 3, 0, 0, 0, 102, 111, 111, 7, 0, 0, 0, 3, 0, 0, 0, 98, 97, 114, 22, 0, 0, 0, 7, 0, 0, 0, 3, 0, 0, 0, 102, 111, 111, 7, 0, 0, 0, 3, 0, 0, 0, 98, 97, 114]), result.GetValue());
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

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Name = new ReadOnlySpan<byte>([15, 15, 15]));
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
