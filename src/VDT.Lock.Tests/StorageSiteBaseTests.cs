using System;
using System.Threading.Tasks;
using Xunit;

namespace VDT.Lock.Tests;

public class StorageSiteBaseTests {
    public class TestStorageSite : StorageSiteBase {
        public TestStorageSite(StorageSettings storageSettings) : base(storageSettings) { }

        protected override Task<SecureBuffer> ExecuteLoad() {
            throw new NotImplementedException();
        }

        protected override Task ExecuteSave(ReadOnlySpan<byte> encryptedData) {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void FieldLengths() {
        var storageSettings = new StorageSettings();
        storageSettings.Set("foo", new ReadOnlySpan<byte>([5, 6, 7, 8, 9]));

        using var subject = new TestStorageSite(storageSettings);

        Assert.Equal([15, 16], subject.FieldLengths);
    }

    [Fact]
    public async Task Load() {
        using var subject = new TestStorageSite(new StorageSettings());

        await Assert.ThrowsAsync<NotImplementedException>(() => subject.Load());
    }

    [Fact]
    public async Task Save() {
        using var subject = new TestStorageSite(new StorageSettings());

        await Assert.ThrowsAsync<NotImplementedException>(() => subject.Save(new ReadOnlySpan<byte>([])));
    }

    [Fact]
    public void Dispose() {
        using var storageSettings = new StorageSettings();

        using (var subject = new TestStorageSite(storageSettings)) { };

        Assert.True(storageSettings.IsDisposed);
    }

    [Fact]
    public void IsDisposed() {
        TestStorageSite disposedSubject;

        using (var subject = new TestStorageSite(new StorageSettings())) {
            disposedSubject = subject;
        };

        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void FieldLengthsThrowsIfDisposed() {
        TestStorageSite disposedSubject;

        using (var subject = new TestStorageSite(new StorageSettings())) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.FieldLengths);
    }

    [Fact]
    public async Task LoadThrowsIfDisposed() {
        TestStorageSite disposedSubject;

        using (var subject = new TestStorageSite(new StorageSettings())) {
            disposedSubject = subject;
        };

        await Assert.ThrowsAsync<ObjectDisposedException>(() => disposedSubject.Load());
    }

    [Fact]
    public async Task SaveThrowsIfDisposed() {
        TestStorageSite disposedSubject;

        using (var subject = new TestStorageSite(new StorageSettings())) {
            disposedSubject = subject;
        };

        await Assert.ThrowsAsync<ObjectDisposedException>(() => disposedSubject.Save(new ReadOnlySpan<byte>([])));
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        TestStorageSite disposedSubject;
        using var plainBytes = new SecureByteList();

        using (var subject = new TestStorageSite(new StorageSettings())) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.SerializeTo(plainBytes));
    }
}
