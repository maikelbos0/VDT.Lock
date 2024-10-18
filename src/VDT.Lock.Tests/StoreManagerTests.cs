using System;
using System.Threading.Tasks;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests;

public class StoreManagerTests {
    public class TestStorageSite : StorageSiteBase {
        private byte[] encryptedData = [];

        public TestStorageSite(StorageSettings storageSettings) : base(storageSettings) { }

        protected override Task<SecureBuffer> ExecuteLoad() {
            return Task.FromResult(new SecureBuffer(encryptedData));
        }

        protected override Task ExecuteSave(ReadOnlySpan<byte> encryptedData) {
            this.encryptedData = encryptedData.ToArray();

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task AuthenticateAndGetPlainStoreKey() {
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

        var randomByteGenerator = new RandomByteGenerator();
        var hashProvider = new HashProvider();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, hashProvider);

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
        var storageSiteFactory = new StorageSiteFactory();
        var hashProvider = new HashProvider();

        using var saveSubject = new StoreManager(encryptor, storageSiteFactory, randomByteGenerator, hashProvider);
        saveSubject.StorageSites.Add(new FileSystemStorageSite("abc"u8));
        saveSubject.StorageSites.Add(new FileSystemStorageSite("def"u8));
        await saveSubject.Authenticate(plainMasterPasswordBuffer);
        using var encryptedBuffer = await saveSubject.SaveStorageSites();

        using var loadSubject = new StoreManager(encryptor, storageSiteFactory, randomByteGenerator, hashProvider);
        await loadSubject.Authenticate(plainMasterPasswordBuffer);
        await loadSubject.LoadStorageSites(encryptedBuffer);

        Assert.Equal(2, loadSubject.StorageSites.Count);
        Assert.Contains(loadSubject.StorageSites, storageSite => storageSite is FileSystemStorageSite fileSystemStorageSite && fileSystemStorageSite.Location.SequenceEqual("abc"u8));
        Assert.Contains(loadSubject.StorageSites, storageSite => storageSite is FileSystemStorageSite fileSystemStorageSite && fileSystemStorageSite.Location.SequenceEqual("def"u8));
    }

    [Fact]
    public async Task LoadStorageSitesThrowsForInvalidBuffer() {
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());
        using var invalidEncryptedBuffer = new SecureBuffer([0, 0, 0, 0]);

        var randomByteGenerator = new RandomByteGenerator();
        var encryptor = new Encryptor(randomByteGenerator);
        var storageSiteFactory = new StorageSiteFactory();
        var hashProvider = new HashProvider();

        using var subject = new StoreManager(encryptor, storageSiteFactory, randomByteGenerator, hashProvider);
        await subject.Authenticate(plainMasterPasswordBuffer);
        await Assert.ThrowsAsync<InvalidAuthenticationException>(() => subject.LoadStorageSites(invalidEncryptedBuffer));
    }

    [Fact]
    public async Task SaveDataStoreAndLoadDataStore() {
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

        var randomByteGenerator = new RandomByteGenerator();
        var encryptor = new Encryptor(randomByteGenerator);
        var storageSiteFactory = new StorageSiteFactory();
        var hashProvider = new HashProvider();

        using var subject = new StoreManager(encryptor, storageSiteFactory, randomByteGenerator, hashProvider) {
            StorageSites = {
                new TestStorageSite(new StorageSettings())
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
    public void EnsureAuthenticatedThrowsIfNotAuthenticated() {
        var randomByteGenerator = new RandomByteGenerator();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider());

        Assert.False(subject.IsAuthenticated);
        Assert.Throws<NotAuthenticatedException>(() => subject.EnsureAuthenticated());
    }

    [Fact]
    public async Task EnsureAuthenticatedDoesNotThrowIfAuthenticated() {
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

        var randomByteGenerator = new RandomByteGenerator();
        var hashProvider = new HashProvider();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, hashProvider);

        using var expectedStoreKey = hashProvider.Provide(plainMasterPasswordBuffer, StoreManager.MasterPasswordSalt);

        await subject.Authenticate(plainMasterPasswordBuffer);

        using var result = await subject.GetPlainStoreKeyBuffer();

        Assert.Equal(expectedStoreKey.Value, result.Value);
    }

    [Fact]
    public async Task SaveStorageSitesThrowsIfNotAuthenticated() {
        var randomByteGenerator = new RandomByteGenerator();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider());

        await Assert.ThrowsAsync<NotAuthenticatedException>(() => subject.SaveStorageSites());
    }

    [Fact]
    public async Task LoadStorageSitesThrowsIfNotAuthenticated() {
        var randomByteGenerator = new RandomByteGenerator();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider());
        using var encryptedBuffer = new SecureBuffer([]);

        await Assert.ThrowsAsync<NotAuthenticatedException>(() => subject.LoadStorageSites(encryptedBuffer));
    }

    [Fact]
    public async Task SaveDataStoreThrowsIfNotAuthenticated() {
        var randomByteGenerator = new RandomByteGenerator();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider());
        using var dataStore = new DataStore();

        await Assert.ThrowsAsync<NotAuthenticatedException>(() => subject.SaveDataStore(dataStore));
    }

    [Fact]
    public async Task LoadDataStoreThrowsIfNotAuthenticated() {
        var randomByteGenerator = new RandomByteGenerator();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider());

        await Assert.ThrowsAsync<NotAuthenticatedException>(() => subject.LoadDataStore());
    }

    [Fact]
    public async Task GetPlainStoreKeyBufferThrowsIfNotAuthenticated() {
        var randomByteGenerator = new RandomByteGenerator();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider());

        await Assert.ThrowsAsync<NotAuthenticatedException>(() => subject.GetPlainStoreKeyBuffer());
    }

    [Fact]
    public async Task Dispose() {
        SecureBuffer plainSessionKeyBuffer;
        SecureBuffer encryptedStoreKeyBuffer;
        DataCollection<StorageSiteBase> storageSites;

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider())) {
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
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        };

        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void IsAuthenticatedThrowsIfDisposed() {
        StoreManager disposedSubject;
        var plainMasterPasswordBuffer = new SecureBuffer(0);

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.IsAuthenticated);
    }

    [Fact]
    public void StorageSitesThrowsIfDisposed() {
        StoreManager disposedSubject;
        var plainMasterPasswordBuffer = new SecureBuffer(0);

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.StorageSites);
    }

    [Fact]
    public async Task AuthenticateThrowsIfDisposed() {
        StoreManager disposedSubject;
        var plainMasterPasswordBuffer = new SecureBuffer(0);

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await disposedSubject.Authenticate(plainMasterPasswordBuffer));
    }

    [Fact]
    public async Task LoadStorageSitesThrowsIfDisposed() {
        StoreManager disposedSubject;
        var encryptedStorageSettingsBuffer = new SecureBuffer(0);

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await disposedSubject.LoadStorageSites(encryptedStorageSettingsBuffer));
    }

    [Fact]
    public async Task SaveStorageSitesThrowsIfDisposed() {
        StoreManager disposedSubject;

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await disposedSubject.SaveStorageSites());
    }

    [Fact]
    public async Task LoadDataStoreThrowsIfDisposed() {
        StoreManager disposedSubject;

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await disposedSubject.LoadDataStore());
    }

    [Fact]
    public async Task SaveDataStoreThrowsIfDisposed() {
        StoreManager disposedSubject;
        using var dataStore = new DataStore();

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await disposedSubject.SaveDataStore(dataStore));
    }

    [Fact]
    public async Task GetPlainStoreKeyBufferThrowsIfDisposed() {
        StoreManager disposedSubject;
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider())) {
            await subject.Authenticate(plainMasterPasswordBuffer);
            disposedSubject = subject;
        }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await disposedSubject.GetPlainStoreKeyBuffer());
    }

    [Fact]
    public void EnsureAuthenticatedThrowsIfDisposed() {
        StoreManager disposedSubject;

        var randomByteGenerator = new RandomByteGenerator();
        using (var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider())) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.EnsureAuthenticated());
    }
}
