using System;
using System.Threading.Tasks;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests;

public class StorageSiteBaseTests {
    public class TestStorageSite : StorageSiteBase {
        public TestStorageSite(ReadOnlySpan<byte> plainNameSpan, StorageSettings storageSettings) : base(plainNameSpan, storageSettings) { }

        protected override Task<SecureBuffer?> ExecuteLoad()
            => Task.FromResult<SecureBuffer?>(new SecureBuffer([]));

        protected override Task<bool> ExecuteSave(SecureBuffer encryptedBuffer)
            => Task.FromResult(true);
    }

    [Fact]
    public void DeserializeFromCreatesFileSystemStorageSite() {
        var result = StorageSiteBase.DeserializeFrom(new ReadOnlySpan<byte>([21, 0, 0, 0, 70, 105, 108, 101, 83, 121, 115, 116, 101, 109, 83, 116, 111, 114, 97, 103, 101, 83, 105, 116, 101, 0, 0, 0, 0, 0, 0, 0, 0]));

        Assert.IsType<FileSystemStorageSite>(result);
    }

    [Fact]
    public void DeserializeFromCreatesChromeStorageSite() {
        var result = StorageSiteBase.DeserializeFrom(new ReadOnlySpan<byte>([17, 0, 0, 0, 67, 104, 114, 111, 109, 101, 83, 116, 111, 114, 97, 103, 101, 83, 105, 116, 101, 0, 0, 0, 0, 0, 0, 0, 0]));

        Assert.IsType<ChromeStorageSite>(result);
    }

    [Fact]
    public void DeserializeFromDeserializesName() {
        var result = StorageSiteBase.DeserializeFrom(new ReadOnlySpan<byte>([21, 0, 0, 0, 70, 105, 108, 101, 83, 121, 115, 116, 101, 109, 83, 116, 111, 114, 97, 103, 101, 83, 105, 116, 101, 4, 0, 0, 0, 110, 97, 109, 101, 0, 0, 0, 0]));

        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), result.Name);
    }

    [Fact]
    public void DeserializeFromDeserializesStorageSettings() {
        var result = StorageSiteBase.DeserializeFrom(new ReadOnlySpan<byte>([21, 0, 0, 0, 70, 105, 108, 101, 83, 121, 115, 116, 101, 109, 83, 116, 111, 114, 97, 103, 101, 83, 105, 116, 101, 0, 0, 0, 0, 21, 0, 0, 0, 8, 0, 0, 0, 76, 111, 99, 97, 116, 105, 111, 110, 5, 0, 0, 0, 118, 97, 108, 117, 101]));

        Assert.Equal(new ReadOnlySpan<byte>([118, 97, 108, 117, 101]), Assert.IsType<FileSystemStorageSite>(result).Location);
    }

    [Fact]
    public void SetName() {
        using var subject = new TestStorageSite(new ReadOnlySpan<byte>([]), new StorageSettings());

        var plainPreviousValueBuffer = subject.GetBuffer<StorageSiteBase>("plainNameBuffer");

        subject.Name = new ReadOnlySpan<byte>([110, 97, 109, 101]);

        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), subject.Name);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }

    [Fact]
    public void FieldLengths() {
        var storageSettings = new StorageSettings();
        storageSettings.Set("location", new ReadOnlySpan<byte>([118, 97, 108, 117, 101]));

        using var subject = new TestStorageSite(new ReadOnlySpan<byte>([110, 97, 109, 101]), storageSettings);

        Assert.Equal([15, 4, 21], subject.FieldLengths);
    }

    [Fact]
    public async Task Load() {
        using var subject = new TestStorageSite(new ReadOnlySpan<byte>([]), new StorageSettings());

        var result = await subject.Load();

        Assert.NotNull(result);
        Assert.Equal([], result.Value);
    }

    [Fact]
    public async Task Save() {
        using var subject = new TestStorageSite(new ReadOnlySpan<byte>([]), new StorageSettings());

        var result = await subject.Save(new SecureBuffer([]));

        Assert.True(result);
    }

    [Fact]
    public void SerializeTo() {
        var storageSettings = new StorageSettings();
        storageSettings.Set("location", new ReadOnlySpan<byte>([118, 97, 108, 117, 101]));

        using var subject = new TestStorageSite(new ReadOnlySpan<byte>([110, 97, 109, 101]), storageSettings);

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([52, 0, 0, 0, 15, 0, 0, 0, 84, 101, 115, 116, 83, 116, 111, 114, 97, 103, 101, 83, 105, 116, 101, 4, 0, 0, 0, 110, 97, 109, 101, 21, 0, 0, 0, 8, 0, 0, 0, 108, 111, 99, 97, 116, 105, 111, 110, 5, 0, 0, 0, 118, 97, 108, 117, 101]), result.GetValue());
    }

    [Fact]
    public void Dispose() {
        TestStorageSite subject;
        SecureBuffer plainNameBuffer;
        using var storageSettings = new StorageSettings();

        using (subject = new TestStorageSite(new ReadOnlySpan<byte>([]), storageSettings)) {
            plainNameBuffer = subject.GetBuffer<StorageSiteBase>("plainNameBuffer");
        }

        Assert.True(subject.IsDisposed);
        Assert.True(plainNameBuffer.IsDisposed);
        Assert.True(storageSettings.IsDisposed);
    }

    [Fact]
    public void GetNameThrowsIfDisposed() {
        TestStorageSite subject;

        using (subject = new TestStorageSite(new ReadOnlySpan<byte>([]), new StorageSettings())) { }

        Assert.Throws<ObjectDisposedException>(() => { var _ = subject.Name; });
    }

    [Fact]
    public void SetNameThrowsIfDisposed() {
        TestStorageSite subject;

        using (subject = new TestStorageSite(new ReadOnlySpan<byte>([]), new StorageSettings())) { }

        Assert.Throws<ObjectDisposedException>(() => subject.Name = new ReadOnlySpan<byte>([15, 15, 15]));
    }

    [Fact]
    public void FieldLengthsThrowsIfDisposed() {
        TestStorageSite subject;

        using (subject = new TestStorageSite(new ReadOnlySpan<byte>([]), new StorageSettings())) { }

        Assert.Throws<ObjectDisposedException>(() => subject.FieldLengths);
    }

    [Fact]
    public async Task LoadThrowsIfDisposed() {
        TestStorageSite subject;

        using (subject = new TestStorageSite(new ReadOnlySpan<byte>([]), new StorageSettings())) { }

        await Assert.ThrowsAsync<ObjectDisposedException>(() => subject.Load());
    }

    [Fact]
    public async Task SaveThrowsIfDisposed() {
        TestStorageSite subject;

        using (subject = new TestStorageSite(new ReadOnlySpan<byte>([]), new StorageSettings())) { }

        await Assert.ThrowsAsync<ObjectDisposedException>(() => subject.Save(new SecureBuffer([])));
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        TestStorageSite subject;
        using var plainBytes = new SecureByteList();

        using (subject = new TestStorageSite(new ReadOnlySpan<byte>([]), new StorageSettings())) { }

        Assert.Throws<ObjectDisposedException>(() => subject.SerializeTo(plainBytes));
    }
}
