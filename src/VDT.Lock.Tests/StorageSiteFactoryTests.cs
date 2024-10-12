using System;
using System.Reflection;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests;

public class StorageSiteFactoryTests {
    [Fact]
    public void DeserializeFromFileSystemStorageSite() {
        var subject = new StorageSiteFactory();

        var result = subject.DeserializeFrom([21, 0, 0, 0, 70, 105, 108, 101, 83, 121, 115, 116, 101, 109, 83, 116, 111, 114, 97, 103, 101, 83, 105, 116, 101, 15, 0, 0, 0, 3, 0, 0, 0, 98, 97, 114, 4, 0, 0, 0, 1, 2, 3, 4]);

        Assert.IsType<FileSystemStorageSite>(result);
        Assert.Equal(new ReadOnlySpan<byte>([1, 2, 3, 4]), GetStorageSettings(result).Get("bar"));
    }

    private static StorageSettings GetStorageSettings(StorageSiteBase storageSite) {
        var storageSettingsField = typeof(StorageSiteBase).GetField("storageSettings", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException();

        return storageSettingsField.GetValue(storageSite) as StorageSettings ?? throw new InvalidOperationException();
    }
}
