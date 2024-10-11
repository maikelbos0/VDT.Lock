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
        var plainSpan = new ReadOnlySpan<byte>([0, 0, 0, 0, 40, 0, 0, 0, 16, 0, 0, 0, 3, 0, 0, 0, 98, 97, 114, 5, 0, 0, 0, 1, 2, 3, 4, 5, 16, 0, 0, 0, 3, 0, 0, 0, 102, 111, 111, 5, 0, 0, 0, 5, 6, 7, 8, 9, 0, 0, 0, 0, 0, 0, 0, 0]);

        using var subject = DataItem.DeserializeFrom(plainSpan);

        Assert.Equal(2, subject.Fields.Count);
        Assert.Contains(subject.Fields, field => field.Name.SequenceEqual(new ReadOnlySpan<byte>([98, 97, 114])) && field.Value.SequenceEqual(new ReadOnlySpan<byte>([1, 2, 3, 4, 5])));
        Assert.Contains(subject.Fields, field => field.Name.SequenceEqual(new ReadOnlySpan<byte>([102, 111, 111])) && field.Value.SequenceEqual(new ReadOnlySpan<byte>([5, 6, 7, 8, 9])));
    }

    [Fact]
    public void DeserializeFromDeserializesLabels() {
        var plainSpan = new ReadOnlySpan<byte>([0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 3, 0, 0, 0, 98, 97, 114, 5, 0, 0, 0, 1, 2, 3, 4, 5, 0, 0, 0, 0]);

        using var subject = DataItem.DeserializeFrom(plainSpan);

        Assert.Equal(2, subject.Labels.Count);
        Assert.Contains(subject.Labels, label => label.Value.SequenceEqual(new ReadOnlySpan<byte>([98, 97, 114])));
        Assert.Contains(subject.Labels, label => label.Value.SequenceEqual(new ReadOnlySpan<byte>([1, 2, 3, 4, 5])));
    }

    [Fact]
    public void DeserializeFromDeserializesLocations() {
        var plainSpan = new ReadOnlySpan<byte>([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 3, 0, 0, 0, 98, 97, 114, 5, 0, 0, 0, 1, 2, 3, 4, 5]);

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
    public void Length() {
        using var subject = new DataItem([98, 97, 114]);
        subject.Fields.Add(new([102, 111, 111], [1, 2, 3, 4, 5]));
        subject.Fields.Add(new([98, 97, 114], [5, 6, 7, 8, 9]));
        subject.Labels.Add(new([102,111,11]));
        subject.Labels.Add(new([98, 97, 114]));
        subject.Locations.Add(new([102,111,11]));
        subject.Locations.Add(new([98, 97, 114]));

        Assert.Equal(79, subject.Length);
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
        };

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Name; });
    }

    [Fact]
    public void SetNameThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Name = new ReadOnlySpan<byte>([15, 15, 15]));
    }

    [Fact]
    public void LabelsThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Labels);
    }

    [Fact]
    public void LocationsThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Locations);
    }

    [Fact]
    public void FieldsThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Fields);
    }

    [Fact]
    public void LengthThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Length);
    }
}
