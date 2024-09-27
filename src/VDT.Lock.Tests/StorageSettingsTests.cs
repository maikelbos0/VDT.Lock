﻿using System;
using System.Collections.Concurrent;
using System.Reflection;
using Xunit;

namespace VDT.Lock.Tests;

public class StorageSettingsTests {
    [Fact]
    public void Constructor() {
        var plainSettingsSpan = new ReadOnlySpan<byte>([
            3, 0, 0, 0, 98, 97, 114,
            4, 0, 0, 0, 1, 2, 3, 4,
            3, 0, 0, 0, 102, 111, 111,
            5, 0, 0, 0, 5, 6, 7, 8, 9
        ]);

        using var subject = new StorageSettings(plainSettingsSpan);

        Assert.Equal(new ReadOnlySpan<byte>([1, 2, 3, 4]), subject.Get("bar"));
        Assert.Equal(new ReadOnlySpan<byte>([5, 6, 7, 8, 9]), subject.Get("foo"));
    }

    [Fact]
    public void SetToAddSetting() {
        var plainSettingsSpan = new ReadOnlySpan<byte>([
            3, 0, 0, 0, 98, 97, 114,
            4, 0, 0, 0, 1, 2, 3, 4,
            3, 0, 0, 0, 102, 111, 111,
            5, 0, 0, 0, 5, 6, 7, 8, 9
        ]);

        using var subject = new StorageSettings(plainSettingsSpan);

        subject.Set("baz", new ReadOnlySpan<byte>([15, 15, 15]));

        Assert.Equal(new ReadOnlySpan<byte>([15, 15, 15]), subject.Get("baz"));
    }

    [Fact]
    public void SetToOverwriteSetting() {
        var plainSettingsSpan = new ReadOnlySpan<byte>([
            3, 0, 0, 0, 98, 97, 114,
            4, 0, 0, 0, 1, 2, 3, 4,
            3, 0, 0, 0, 102, 111, 111,
            5, 0, 0, 0, 5, 6, 7, 8, 9
        ]);

        using var subject = new StorageSettings(plainSettingsSpan);
        
        var previousValue = GetSettings(subject)["foo"];

        subject.Set("foo", new ReadOnlySpan<byte>([15, 15, 15]));

        Assert.Equal(new ReadOnlySpan<byte>([15, 15, 15]), subject.Get("foo"));
        Assert.True(previousValue.IsDisposed);
    }

    [Fact]
    public void Serialize() {
        var plainSettingsSpan = new ReadOnlySpan<byte>([
            3, 0, 0, 0, 98, 97, 114,
            4, 0, 0, 0, 1, 2, 3, 4,
            3, 0, 0, 0, 102, 111, 111,
            5, 0, 0, 0, 5, 6, 7, 8, 9
        ]);

        using var subject = new StorageSettings(plainSettingsSpan);

        using var plainSettingsBuffer = subject.Serialize();

        Assert.Equal(plainSettingsSpan, plainSettingsBuffer.Value);
    }

    [Fact]
    public void Dispose() {
        var plainSettingsSpan = new ReadOnlySpan<byte>([
            3, 0, 0, 0, 98, 97, 114,
            4, 0, 0, 0, 1, 2, 3, 4,
            3, 0, 0, 0, 102, 111, 111,
            5, 0, 0, 0, 5, 6, 7, 8, 9
        ]);
        ConcurrentDictionary<string, SecureBuffer> settings;
        
        using (var subject = new StorageSettings(plainSettingsSpan)) {
            settings = GetSettings(subject);
        };

        Assert.Equal(2, settings.Count);

        foreach (var buffer in settings.Values) {
            Assert.True(buffer.IsDisposed);
        }
    }

    private static ConcurrentDictionary<string, SecureBuffer> GetSettings(StorageSettings storageSettings) {
        var settingsField = typeof(StorageSettings).GetField("settings", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException();
        
        return settingsField.GetValue(storageSettings) as ConcurrentDictionary<string, SecureBuffer> ?? throw new InvalidOperationException();
    }
}
