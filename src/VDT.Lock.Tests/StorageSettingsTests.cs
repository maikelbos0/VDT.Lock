using System;
using System.Collections.Generic;
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
    public void Length() {
        using var subject = new StorageSettings();

        subject.Set("foo", new ReadOnlySpan<byte>([5, 6, 7, 8, 9]));
        subject.Set("bar", new ReadOnlySpan<byte>([1, 2, 3, 4, 5]));

        Assert.Equal(32, subject.Length);
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
        Dictionary<string, SecureBuffer> plainSettingsBuffer;

        using (var subject = new StorageSettings()) {
            subject.Set("foo", new ReadOnlySpan<byte>([5, 6, 7, 8, 9]));
            subject.Set("bar", new ReadOnlySpan<byte>([1, 2, 3, 4]));

            plainSettingsBuffer = GetSettings(subject);
        };

        Assert.Equal(2, plainSettingsBuffer.Count);

        foreach (var buffer in plainSettingsBuffer.Values) {
            Assert.True(buffer.IsDisposed);
        }
    }

    [Fact]
    public void IsDisposed() {
        StorageSettings disposedSubject;

        using (var subject = new StorageSettings()) {
            disposedSubject = subject;
        };

        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void LengthThrowsIfDisposed() {
        StorageSettings disposedSubject;

        using (var subject = new StorageSettings()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Length);
    }

    [Fact]
    public void GetThrowsIfDisposed() {
        StorageSettings disposedSubject;

        using (var subject = new StorageSettings()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Get("foo"));
    }

    [Fact]
    public void SetThrowsIfDisposed() {
        StorageSettings disposedSubject;

        using (var subject = new StorageSettings()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Set("foo", new ReadOnlySpan<byte>([])));
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        StorageSettings disposedSubject;
        using var plainBytes = new SecureByteList();

        using (var subject = new StorageSettings()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.SerializeTo(plainBytes));
    }

    private static Dictionary<string, SecureBuffer> GetSettings(StorageSettings storageSettings) {
        var settingsField = typeof(StorageSettings).GetField("plainSettingsBuffers", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException();

        return settingsField.GetValue(storageSettings) as Dictionary<string, SecureBuffer> ?? throw new InvalidOperationException();
    }
}
