using System;
using System.Threading.Tasks;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests;

public class StoreManagerTests {
    public class TestStorageSite : StorageSiteBase {
        private byte[] encryptedData = [];

        public TestStorageSite(ReadOnlySpan<byte> plainNameSpan, StorageSettings storageSettings) : base(plainNameSpan, storageSettings) { }

        protected override Task<SecureBuffer?> ExecuteLoad() {
            return Task.FromResult<SecureBuffer?>(new SecureBuffer(encryptedData));
        }

        protected override Task<bool> ExecuteSave(ReadOnlySpan<byte> encryptedData) {
            this.encryptedData = encryptedData.ToArray();

            return Task.FromResult(true);
        }
    }

    [Fact]
    public void SetStorageSites() {
        var randomByteGenerator = new RandomByteGenerator();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider());

        var previousStorageSites = subject.StorageSites;
        var newStorageSites = new DataCollection<StorageSiteBase>();

        subject.StorageSites = newStorageSites;

        Assert.Same(newStorageSites, subject.StorageSites);
        Assert.True(previousStorageSites.IsDisposed);
    }

    [Fact]
    public async Task AuthenticateAndGetPlainStoreKey() {
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

        var randomByteGenerator = new RandomByteGenerator();
        var hashProvider = new HashProvider();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, hashProvider);

        using var expectedStoreKey = hashProvider.Provide(plainMasterPasswordBuffer, StoreManager.MasterPasswordSalt);

        await subject.Authenticate(plainMasterPasswordBuffer);

        using var result = await subject.GetPlainStoreKeyBuffer();

        Assert.Equal(expectedStoreKey.Value, result.Value);
    }

    [Fact]
    public async Task SaveStorageSitesAndLoadStorageSites() {
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

        var randomByteGenerator = new RandomByteGenerator();
        var encryptor = new Encryptor(randomByteGenerator);
        var hashProvider = new HashProvider();

        using var saveSubject = new StoreManager(encryptor, randomByteGenerator, hashProvider);
        saveSubject.StorageSites.Add(new FileSystemStorageSite("foo"u8, "abc"u8));
        saveSubject.StorageSites.Add(new FileSystemStorageSite("bar"u8, "def"u8));
        await saveSubject.Authenticate(plainMasterPasswordBuffer);
        using var encryptedBuffer = await saveSubject.SaveStorageSites();

        using var loadSubject = new StoreManager(encryptor, randomByteGenerator, hashProvider);
        await loadSubject.Authenticate(plainMasterPasswordBuffer);
        await loadSubject.LoadStorageSites(encryptedBuffer);

        Assert.Equal(2, loadSubject.StorageSites.Count);
        Assert.Contains(loadSubject.StorageSites, storageSite => storageSite is FileSystemStorageSite fileSystemStorageSite && fileSystemStorageSite.Name.SequenceEqual("foo"u8) && fileSystemStorageSite.Location.SequenceEqual("abc"u8));
        Assert.Contains(loadSubject.StorageSites, storageSite => storageSite is FileSystemStorageSite fileSystemStorageSite && fileSystemStorageSite.Name.SequenceEqual("bar"u8) && fileSystemStorageSite.Location.SequenceEqual("def"u8));
    }

    [Fact]
    public async Task LoadStorageSitesThrowsForInvalidBuffer() {
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());
        using var invalidEncryptedBuffer = new SecureBuffer([0, 0, 0, 0]);

        var randomByteGenerator = new RandomByteGenerator();
        var encryptor = new Encryptor(randomByteGenerator);
        var hashProvider = new HashProvider();

        using var subject = new StoreManager(encryptor, randomByteGenerator, hashProvider);
        await subject.Authenticate(plainMasterPasswordBuffer);
        await Assert.ThrowsAsync<InvalidAuthenticationException>(() => subject.LoadStorageSites(invalidEncryptedBuffer));
    }

    [Fact]
    public async Task SaveDataStoreAndLoadDataStore() {
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

        var randomByteGenerator = new RandomByteGenerator();
        var encryptor = new Encryptor(randomByteGenerator);
        var hashProvider = new HashProvider();

        using var subject = new StoreManager(encryptor, randomByteGenerator, hashProvider) {
            StorageSites = {
                new TestStorageSite("foo"u8, new StorageSettings())
            }
        };

        using var dataStore = new DataStore([102, 111, 111]) {
            Items = {
                new([98, 97, 114])
            }
        };

        await subject.Authenticate(plainMasterPasswordBuffer);
        await subject.SaveDataStore(dataStore);

        var result = await subject.LoadDataStore();

        Assert.Equal(new ReadOnlySpan<byte>([102, 111, 111]), result.Name);
        Assert.Equal(new ReadOnlySpan<byte>([98, 97, 114]), Assert.Single(result.Items).Name);
    }

    [Fact]
    public async Task EnsureAuthenticatedDoesNotThrowIfAuthenticated() {
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

        var randomByteGenerator = new RandomByteGenerator();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider());

        await subject.Authenticate(plainMasterPasswordBuffer);

        Assert.True(subject.IsAuthenticated);
        subject.EnsureAuthenticated();
    }

    [Fact]
    public void EnsureAuthenticatedThrowsIfNotAuthenticated() {
        var randomByteGenerator = new RandomByteGenerator();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider());

        Assert.False(subject.IsAuthenticated);
        Assert.Throws<NotAuthenticatedException>(() => subject.EnsureAuthenticated());
    }

    [Fact]
    public async Task SaveStorageSitesThrowsIfNotAuthenticated() {
        var randomByteGenerator = new RandomByteGenerator();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider());

        await Assert.ThrowsAsync<NotAuthenticatedException>(() => subject.SaveStorageSites());
    }

    [Fact]
    public async Task LoadStorageSitesThrowsIfNotAuthenticated() {
        var randomByteGenerator = new RandomByteGenerator();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider());
        using var encryptedBuffer = new SecureBuffer([]);

        await Assert.ThrowsAsync<NotAuthenticatedException>(() => subject.LoadStorageSites(encryptedBuffer));
    }

    [Fact]
    public async Task SaveDataStoreThrowsIfNotAuthenticated() {
        var randomByteGenerator = new RandomByteGenerator();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider());
        using var dataStore = new DataStore();

        await Assert.ThrowsAsync<NotAuthenticatedException>(() => subject.SaveDataStore(dataStore));
    }

    [Fact]
    public async Task LoadDataStoreThrowsIfNotAuthenticated() {
        var randomByteGenerator = new RandomByteGenerator();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider());

        await Assert.ThrowsAsync<NotAuthenticatedException>(() => subject.LoadDataStore());
    }

    [Fact]
    public async Task GetPlainStoreKeyBufferThrowsIfNotAuthenticated() {
        var randomByteGenerator = new RandomByteGenerator();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider());

        await Assert.ThrowsAsync<NotAuthenticatedException>(() => subject.GetPlainStoreKeyBuffer());
    }

    [Fact]
    public async Task Dispose() {
        SecureBuffer plainSessionKeyBuffer;
        SecureBuffer encryptedStoreKeyBuffer;
        DataCollection<StorageSiteBase> storageSites;

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider())) {
            using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

            await subject.Authenticate(plainMasterPasswordBuffer);

            plainSessionKeyBuffer = subject.GetBuffer("plainSessionKeyBuffer");
            encryptedStoreKeyBuffer = subject.GetBuffer("encryptedStoreKeyBuffer");
            storageSites = subject.StorageSites;
        }

        Assert.True(plainSessionKeyBuffer.IsDisposed);
        Assert.True(encryptedStoreKeyBuffer.IsDisposed);
        Assert.True(storageSites.IsDisposed);
    }

    [Fact]
    public void IsDisposed() {
        StoreManager disposedSubject;

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void IsAuthenticatedThrowsIfDisposed() {
        StoreManager disposedSubject;

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.IsAuthenticated);
    }

    [Fact]
    public void GetStorageSitesThrowsIfDisposed() {
        StoreManager disposedSubject;

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.StorageSites);
    }

    [Fact]
    public void SetStorageSitesThrowsIfDisposed() {
        StoreManager disposedSubject;

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.StorageSites = []);
    }

    [Fact]
    public async Task AuthenticateThrowsIfDisposed() {
        StoreManager disposedSubject;
        var plainMasterPasswordBuffer = new SecureBuffer(0);

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await disposedSubject.Authenticate(plainMasterPasswordBuffer));
    }

    [Fact]
    public async Task LoadStorageSitesThrowsIfDisposed() {
        StoreManager disposedSubject;
        var encryptedStorageSettingsBuffer = new SecureBuffer(0);

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await disposedSubject.LoadStorageSites(encryptedStorageSettingsBuffer));
    }

    [Fact]
    public async Task SaveStorageSitesThrowsIfDisposed() {
        StoreManager disposedSubject;

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await disposedSubject.SaveStorageSites());
    }

    [Fact]
    public async Task LoadDataStoreThrowsIfDisposed() {
        StoreManager disposedSubject;

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await disposedSubject.LoadDataStore());
    }

    [Fact]
    public async Task SaveDataStoreThrowsIfDisposed() {
        StoreManager disposedSubject;
        using var dataStore = new DataStore();

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await disposedSubject.SaveDataStore(dataStore));
    }

    [Fact]
    public async Task GetPlainStoreKeyBufferThrowsIfDisposed() {
        StoreManager disposedSubject;

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await disposedSubject.GetPlainStoreKeyBuffer());
    }

    [Fact]
    public void EnsureAuthenticatedThrowsIfDisposed() {
        StoreManager disposedSubject;

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.EnsureAuthenticated());
    }
}
