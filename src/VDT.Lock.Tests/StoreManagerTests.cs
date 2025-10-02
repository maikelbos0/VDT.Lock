using System;
using System.Threading.Tasks;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests;

public class StoreManagerTests {
    public class TestStorageSite : StorageSiteBase {
        private byte[] encryptedBytes = [];

        public TestStorageSite(ReadOnlySpan<byte> plainNameSpan, StorageSettings storageSettings) : base(plainNameSpan, storageSettings) { }

        protected override Task<SecureBuffer?> ExecuteLoad() {
            return Task.FromResult<SecureBuffer?>(new SecureBuffer(encryptedBytes));
        }

        protected override Task<bool> ExecuteSave(SecureBuffer encryptedBuffer) {
            encryptedBytes = [.. encryptedBuffer.Value];

            return Task.FromResult(true);
        }
    }

    [Fact]
    public void SetStorageSites() {
        using var subject = new StoreManager(new Encryptor(), new HashProvider());

        var previousStorageSites = subject.StorageSites;
        var newStorageSites = new DataCollection<StorageSiteBase>();

        subject.StorageSites = newStorageSites;

        Assert.Same(newStorageSites, subject.StorageSites);
        Assert.True(previousStorageSites.IsDisposed);
    }

    [Fact]
    public async Task AuthenticateAndGetPlainStoreKey() {
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

        var hashProvider = new HashProvider();
        using var subject = new StoreManager(new Encryptor(), hashProvider);

        using var expectedStoreKey = hashProvider.Provide(plainMasterPasswordBuffer, StoreManager.MasterPasswordSalt);

        await subject.Authenticate(plainMasterPasswordBuffer);

        using var result = await subject.GetPlainStoreKeyBuffer();

        Assert.Equal(expectedStoreKey.Value, result.Value);
    }

    [Fact]
    public async Task SaveStorageSitesAndLoadStorageSites() {
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

        var encryptor = new Encryptor();
        var hashProvider = new HashProvider();

        using var saveSubject = new StoreManager(encryptor, hashProvider);
        saveSubject.StorageSites.Add(new FileSystemStorageSite("foo"u8, "abc"u8));
        saveSubject.StorageSites.Add(new FileSystemStorageSite("bar"u8, "def"u8));
        await saveSubject.Authenticate(plainMasterPasswordBuffer);
        using var encryptedBuffer = await saveSubject.SaveStorageSites();

        using var loadSubject = new StoreManager(encryptor, hashProvider);
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

        using var subject = new StoreManager(new Encryptor(), new HashProvider());

        await subject.Authenticate(plainMasterPasswordBuffer);
        await Assert.ThrowsAsync<InvalidAuthenticationException>(() => subject.LoadStorageSites(invalidEncryptedBuffer));
    }

    [Fact]
    public async Task SaveDataStoreAndLoadDataStore() {
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

        using var subject = new StoreManager(new Encryptor(), new HashProvider()) {
            StorageSites = {
                new TestStorageSite([98, 97, 114], new StorageSettings())
            }
        };

        using var dataStore = new DataStore([102, 111, 111]) {
            Items = {
                new([98, 97, 114])
            }
        };

        await subject.Authenticate(plainMasterPasswordBuffer);
        var saveResult = await subject.SaveDataStore(dataStore);

        Assert.Equal(new ReadOnlySpan<byte>([98, 97, 114]), Assert.Single(saveResult.SucceededStorageSites).Value);
        Assert.Empty(saveResult.FailedStorageSites);

        var (dataStoreResult, loadResult) = await subject.LoadDataStore();

        Assert.Equal(new ReadOnlySpan<byte>([102, 111, 111]), dataStoreResult.Name);
        Assert.Equal(new ReadOnlySpan<byte>([98, 97, 114]), Assert.Single(dataStoreResult.Items).Name);
        Assert.Equal(new ReadOnlySpan<byte>([98, 97, 114]), Assert.Single(loadResult.SucceededStorageSites).Value);
        Assert.Empty(loadResult.FailedStorageSites);
    }

    [Fact]
    public async Task EnsureAuthenticatedDoesNotThrowIfAuthenticated() {
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

        using var subject = new StoreManager(new Encryptor(), new HashProvider());

        await subject.Authenticate(plainMasterPasswordBuffer);

        Assert.True(subject.IsAuthenticated);
        subject.EnsureAuthenticated();
    }

    [Fact]
    public void EnsureAuthenticatedThrowsIfNotAuthenticated() {
        using var subject = new StoreManager(new Encryptor(), new HashProvider());

        Assert.False(subject.IsAuthenticated);
        Assert.Throws<NotAuthenticatedException>(() => subject.EnsureAuthenticated());
    }

    [Fact]
    public async Task SaveStorageSitesThrowsIfNotAuthenticated() {
        using var subject = new StoreManager(new Encryptor(), new HashProvider());

        await Assert.ThrowsAsync<NotAuthenticatedException>(() => subject.SaveStorageSites());
    }

    [Fact]
    public async Task LoadStorageSitesThrowsIfNotAuthenticated() {
        using var subject = new StoreManager(new Encryptor(), new HashProvider());
        using var encryptedBuffer = new SecureBuffer([]);

        await Assert.ThrowsAsync<NotAuthenticatedException>(() => subject.LoadStorageSites(encryptedBuffer));
    }

    [Fact]
    public async Task SaveDataStoreThrowsIfNotAuthenticated() {
        using var subject = new StoreManager(new Encryptor(), new HashProvider());
        using var dataStore = new DataStore();

        await Assert.ThrowsAsync<NotAuthenticatedException>(() => subject.SaveDataStore(dataStore));
    }

    [Fact]
    public async Task LoadDataStoreThrowsIfNotAuthenticated() {
        using var subject = new StoreManager(new Encryptor(), new HashProvider());

        await Assert.ThrowsAsync<NotAuthenticatedException>(() => subject.LoadDataStore());
    }

    [Fact]
    public async Task GetPlainStoreKeyBufferThrowsIfNotAuthenticated() {
        using var subject = new StoreManager(new Encryptor(), new HashProvider());

        await Assert.ThrowsAsync<NotAuthenticatedException>(() => subject.GetPlainStoreKeyBuffer());
    }

    [Fact]
    public async Task Dispose() {
        StoreManager subject;
        SecureBuffer plainSessionKeyBuffer;
        SecureBuffer encryptedStoreKeyBuffer;
        DataCollection<StorageSiteBase> storageSites;

        using (subject = new(new Encryptor(), new HashProvider())) {
            using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

            await subject.Authenticate(plainMasterPasswordBuffer);

            plainSessionKeyBuffer = subject.GetBuffer("plainSessionKeyBuffer");
            encryptedStoreKeyBuffer = subject.GetBuffer("encryptedStoreKeyBuffer");
            storageSites = subject.StorageSites;
        }

        Assert.True(subject.IsDisposed);
        Assert.True(plainSessionKeyBuffer.IsDisposed);
        Assert.True(encryptedStoreKeyBuffer.IsDisposed);
        Assert.True(storageSites.IsDisposed);
    }

    [Fact]
    public void IsAuthenticatedThrowsIfDisposed() {
        StoreManager subject;

        using (subject = new(new Encryptor(), new HashProvider())) { }

        Assert.Throws<ObjectDisposedException>(() => subject.IsAuthenticated);
    }

    [Fact]
    public void GetStorageSitesThrowsIfDisposed() {
        StoreManager subject;

        using (subject = new(new Encryptor(), new HashProvider())) { }

        Assert.Throws<ObjectDisposedException>(() => subject.StorageSites);
    }

    [Fact]
    public void SetStorageSitesThrowsIfDisposed() {
        StoreManager subject;

        using (subject = new(new Encryptor(), new HashProvider())) { }

        Assert.Throws<ObjectDisposedException>(() => subject.StorageSites = []);
    }

    [Fact]
    public async Task AuthenticateThrowsIfDisposed() {
        StoreManager subject;
        var plainMasterPasswordBuffer = new SecureBuffer(0);

        using (subject = new(new Encryptor(), new HashProvider())) { }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await subject.Authenticate(plainMasterPasswordBuffer));
    }

    [Fact]
    public async Task LoadStorageSitesThrowsIfDisposed() {
        StoreManager subject;
        var encryptedStorageSettingsBuffer = new SecureBuffer(0);

        using (subject = new(new Encryptor(), new HashProvider())) { }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await subject.LoadStorageSites(encryptedStorageSettingsBuffer));
    }

    [Fact]
    public async Task SaveStorageSitesThrowsIfDisposed() {
        StoreManager subject;

        using (subject = new(new Encryptor(), new HashProvider())) { }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await subject.SaveStorageSites());
    }

    [Fact]
    public async Task LoadDataStoreThrowsIfDisposed() {
        StoreManager subject;

        using (subject = new(new Encryptor(), new HashProvider())) { }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await subject.LoadDataStore());
    }

    [Fact]
    public async Task SaveDataStoreThrowsIfDisposed() {
        StoreManager subject;
        using var dataStore = new DataStore();

        using (subject = new(new Encryptor(), new HashProvider())) { }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await subject.SaveDataStore(dataStore));
    }

    [Fact]
    public async Task GetPlainStoreKeyBufferThrowsIfDisposed() {
        StoreManager subject;

        using (subject = new(new Encryptor(), new HashProvider())) { }

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await subject.GetPlainStoreKeyBuffer());
    }

    [Fact]
    public void EnsureAuthenticatedThrowsIfDisposed() {
        StoreManager subject;

        using (subject = new(new Encryptor(), new HashProvider())) { }

        Assert.Throws<ObjectDisposedException>(() => subject.EnsureAuthenticated());
    }
}
