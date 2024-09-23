using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace VDT.Lock.Tests;

public class StorageSettingsTests {
    [Fact]
    public void Constructor() {
        var settingsSpan = new ReadOnlySpan<byte>([
            3, 0, 0, 0, 98, 97, 114,
            4, 0, 0, 0, 1, 2, 3, 4,
            3, 0, 0, 0, 102, 111, 111,
            5, 0, 0, 0, 5, 6, 7, 8, 9
        ]);

        using var subject = new StorageSettings(settingsSpan);

        Assert.Equal(new ReadOnlySpan<byte>([1, 2, 3, 4]), subject.GetSetting("bar"));
        Assert.Equal(new ReadOnlySpan<byte>([5, 6, 7, 8, 9]), subject.GetSetting("foo"));
    }

    [Fact]
    public void Dispose() {
        var settingsSpan = new ReadOnlySpan<byte>([
            3, 0, 0, 0, 98, 97, 114,
            4, 0, 0, 0, 1, 2, 3, 4,
            3, 0, 0, 0, 102, 111, 111,
            5, 0, 0, 0, 5, 6, 7, 8, 9
        ]);
        Dictionary<string, SecureBuffer> settings;
        
        using (var subject = new StorageSettings(settingsSpan)) {
            var settingsField = typeof(StorageSettings).GetField("settings", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException();
            settings = settingsField.GetValue(subject) as Dictionary<string, SecureBuffer> ?? throw new InvalidOperationException();
        };

        Assert.Equal(2, settings.Count);

        foreach (var buffer in settings.Values) {
            Assert.True(buffer.IsDisposed);
        }
    }
}
