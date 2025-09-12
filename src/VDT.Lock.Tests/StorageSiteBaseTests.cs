using System;
using System.Threading.Tasks;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests;

public class StorageSiteBaseTests {
    public class TestStorageSite : StorageSiteBase {
        public TestStorageSite(ReadOnlySpan<byte> plainNameSpan, StorageSettings storageSettings) : base(plainNameSpan, storageSettings) { }

        protected override Task<SecureBuffer> ExecuteLoad() {
            throw new NotImplementedException();
        }

        protected override Task ExecuteSave(ReadOnlySpan<byte> encryptedData) {
            throw new NotImplementedException();
        }
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
        var result = StorageSiteBase.DeserializeFrom(new ReadOnlySpan<byte>([21, 0, 0, 0, 70, 105, 108, 101, 83, 121, 115, 116, 101, 109, 83, 116, 111, 114, 97, 103, 101, 83, 105, 116, 101, 3, 0, 0, 0, 102, 111, 111, 0, 0, 0, 0]));

        Assert.Equal(new ReadOnlySpan<byte>([102, 111, 111]), result.Name);
    }

    [Fact]
    public void DeserializeFromDeserializesStorageSettings() {
        var result = StorageSiteBase.DeserializeFrom(new ReadOnlySpan<byte>([21, 0, 0, 0, 70, 105, 108, 101, 83, 121, 115, 116, 101, 109, 83, 116, 111, 114, 97, 103, 101, 83, 105, 116, 101, 0, 0, 0, 0, 21, 0, 0, 0, 8, 0, 0, 0, 76, 111, 99, 97, 116, 105, 111, 110, 5, 0, 0, 0, 1, 2, 3, 4, 5]));

        Assert.Equal(new ReadOnlySpan<byte>([1, 2, 3, 4, 5]), Assert.IsType<FileSystemStorageSite>(result).Location);
    }

    [Fact]
    public void SetName() {
        using var subject = new TestStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), new StorageSettings());

        var plainPreviousValueBuffer = subject.GetBuffer<StorageSiteBase>("plainNameBuffer");

        subject.Name = new ReadOnlySpan<byte>([99, 99, 99]);

        Assert.Equal(new ReadOnlySpan<byte>([99, 99, 99]), subject.Name);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }

    [Fact]
    public void FieldLengths() {
        var storageSettings = new StorageSettings();
        storageSettings.Set("foo", new ReadOnlySpan<byte>([5, 6, 7, 8, 9]));

        using var subject = new TestStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), storageSettings);

        Assert.Equal([15, 3, 16], subject.FieldLengths);
    }

    [Fact]
    public async Task Load() {
        using var subject = new TestStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), new StorageSettings());

        await Assert.ThrowsAsync<NotImplementedException>(() => subject.Load());
    }

    [Fact]
    public async Task Save() {
        using var subject = new TestStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), new StorageSettings());

        await Assert.ThrowsAsync<NotImplementedException>(() => subject.Save(new ReadOnlySpan<byte>([])));
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new TestStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), new StorageSettings());

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([30, 0, 0, 0, 15, 0, 0, 0, 84, 101, 115, 116, 83, 116, 111, 114, 97, 103, 101, 83, 105, 116, 101, 3, 0, 0, 0, 102, 111, 111, 0, 0, 0, 0]), result.GetValue());
    }

    [Fact]
    public void Dispose() {
        using var storageSettings = new StorageSettings();

        using (var subject = new TestStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), storageSettings)) { }

        Assert.True(storageSettings.IsDisposed);
    }

    [Fact]
    public void IsDisposed() {
        TestStorageSite disposedSubject;

        using (var subject = new TestStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), new StorageSettings())) {
            disposedSubject = subject;
        }

        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void GetNameThrowsIfDisposed() {
        TestStorageSite disposedSubject;

        using (var subject = new TestStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), new StorageSettings())) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Name; });
    }

    [Fact]
    public void SetNameThrowsIfDisposed() {
        TestStorageSite disposedSubject;

        using (var subject = new TestStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), new StorageSettings())) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Name = new ReadOnlySpan<byte>([15, 15, 15]));
    }

    [Fact]
    public void FieldLengthsThrowsIfDisposed() {
        TestStorageSite disposedSubject;

        using (var subject = new TestStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), new StorageSettings())) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.FieldLengths);
    }

    [Fact]
    public async Task LoadThrowsIfDisposed() {
        TestStorageSite disposedSubject;

        using (var subject = new TestStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), new StorageSettings())) {
            disposedSubject = subject;
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(() => disposedSubject.Load());
    }

    [Fact]
    public async Task SaveThrowsIfDisposed() {
        TestStorageSite disposedSubject;

        using (var subject = new TestStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), new StorageSettings())) {
            disposedSubject = subject;
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(() => disposedSubject.Save(new ReadOnlySpan<byte>([])));
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        TestStorageSite disposedSubject;
        using var plainBytes = new SecureByteList();

        using (var subject = new TestStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), new StorageSettings())) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.SerializeTo(plainBytes));
    }
}
