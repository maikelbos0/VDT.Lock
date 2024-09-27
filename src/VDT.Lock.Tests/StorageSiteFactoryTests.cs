using System;
using System.Reflection;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests;

public class StorageSiteFactoryTests
{
    [Theory]
    [InlineData(nameof(FileSystemStorageSite), typeof(FileSystemStorageSite))]
    public void Create(string typeName, Type expectedType) {
        var storageSettings = new StorageSettings([]);

        var subject = new StorageSiteFactory();

        var result = subject.Create(typeName, storageSettings);

        Assert.IsType(expectedType, result);
        Assert.Same(storageSettings, GetStorageSettings(result));
    }

    private static StorageSettings GetStorageSettings(StorageSiteBase storageSite) {
        var storageSettingsField = typeof(StorageSiteBase).GetField("storageSettings", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException();

        return storageSettingsField.GetValue(storageSite) as StorageSettings ?? throw new InvalidOperationException();
    }
}
