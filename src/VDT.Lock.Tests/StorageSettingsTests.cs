using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace VDT.Lock.Tests;

public class StorageSettingsTests {
    [Fact]
    public void DeserializeFrom() {
        var plainSettingsSpan = new ReadOnlySpan<byte>([3, 0, 0, 0, 98, 97, 114, 4, 0, 0, 0, 1, 2, 3, 4, 3, 0, 0, 0, 102, 111, 111, 5, 0, 0, 0, 5, 6, 7, 8, 9]);

        using var subject = StorageSettings.DeserializeFrom(plainSettingsSpan);

        Assert.Equal(new ReadOnlySpan<byte>([1, 2, 3, 4]), subject.Get("bar"));
        Assert.Equal(new ReadOnlySpan<byte>([5, 6, 7, 8, 9]), subject.Get("foo"));
    }

    [Fact]
    public void FieldLengths() {
        using var subject = new StorageSettings();

        subject.Set("foo", new ReadOnlySpan<byte>([5, 6, 7, 8, 9]));
        subject.Set("bar", new ReadOnlySpan<byte>([1, 2, 3, 4, 5]));

        Assert.Equal([3, 5, 3, 5], subject.FieldLengths);
    }

    [Fact]
    public void SetToAddSetting() {
        using var subject = new StorageSettings();

        subject.Set("foo", new ReadOnlySpan<byte>([5, 6, 7, 8, 9]));

        Assert.Equal(new ReadOnlySpan<byte>([5, 6, 7, 8, 9]), subject.Get("foo"));
    }

    [Fact]
    public void SetToOverwriteSetting() {
        using var subject = new StorageSettings();

        subject.Set("foo", new ReadOnlySpan<byte>([5, 6, 7, 8, 9]));

        var plainPreviousValueBuffer = GetSettings(subject)["foo"];

        subject.Set("foo", new ReadOnlySpan<byte>([15, 15, 15]));

        Assert.Equal(new ReadOnlySpan<byte>([15, 15, 15]), subject.Get("foo"));
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new StorageSettings();

        subject.Set("foo", new ReadOnlySpan<byte>([5, 6, 7, 8, 9]));
        subject.Set("bar", new ReadOnlySpan<byte>([1, 2, 3, 4]));

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([31, 0, 0, 0, 3, 0, 0, 0, 98, 97, 114, 4, 0, 0, 0, 1, 2, 3, 4, 3, 0, 0, 0, 102, 111, 111, 5, 0, 0, 0, 5, 6, 7, 8, 9]), result.GetValue());
    }

    [Fact]
    public void Dispose() {
        StorageSettings subject;
        Dictionary<string, SecureBuffer> plainSettingsBuffer;

        using (subject = new()) {
            subject.Set("foo", new ReadOnlySpan<byte>([5, 6, 7, 8, 9]));
            subject.Set("bar", new ReadOnlySpan<byte>([1, 2, 3, 4]));

            plainSettingsBuffer = GetSettings(subject);
        }

        Assert.True(subject.IsDisposed);

        Assert.Equal(2, plainSettingsBuffer.Count);

        foreach (var buffer in plainSettingsBuffer.Values) {
            Assert.True(buffer.IsDisposed);
        }
    }

    [Fact]
    public void FieldLengthsThrowsIfDisposed() {
        StorageSettings subject;

        using (subject = new()) { }

        // The enumerable is lazily evaluated here so we need to materialize it
        Assert.Throws<ObjectDisposedException>(() => subject.FieldLengths.ToList());
    }

    [Fact]
    public void GetThrowsIfDisposed() {
        StorageSettings subject;

        using (subject = new()) { }

        Assert.Throws<ObjectDisposedException>(() => subject.Get("foo"));
    }

    [Fact]
    public void SetThrowsIfDisposed() {
        StorageSettings subject;

        using (subject = new()) { }

        Assert.Throws<ObjectDisposedException>(() => subject.Set("foo", new ReadOnlySpan<byte>([])));
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        StorageSettings subject;
        using var plainBytes = new SecureByteList();

        using (subject = new()) { }

        Assert.Throws<ObjectDisposedException>(() => subject.SerializeTo(plainBytes));
    }

    private static Dictionary<string, SecureBuffer> GetSettings(StorageSettings storageSettings) {
        var settingsField = typeof(StorageSettings).GetField("plainSettingsBuffers", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException();

        return settingsField.GetValue(storageSettings) as Dictionary<string, SecureBuffer> ?? throw new InvalidOperationException();
    }
}
