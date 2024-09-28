using System.Threading.Tasks;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests;

public class StoreManagerTests {
    [Fact]
    public async Task AuthenticateAndGetPlainStoreKey() {
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

        var randomByteGenerator = new RandomByteGenerator();
        var hashProvider = new HashProvider();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, hashProvider);

        using var expectedStoreKey = hashProvider.Provide(plainMasterPasswordBuffer, StoreManagerFactory.MasterPasswordSalt);

        await subject.Authenticate(plainMasterPasswordBuffer);

        using var result = await subject.GetPlainStoreKeyBuffer();

        Assert.Equal(expectedStoreKey.Value, result.Value);
    }

    [Fact]
    public void EnsureAuthenticatedThrowsWhenNotAuthenticated() {
        var randomByteGenerator = new RandomByteGenerator();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, new HashProvider());

        Assert.False(subject.IsAuthenticated);
        Assert.Throws<NotAuthenticatedException>(() => subject.EnsureAuthenticated());
    }

    [Fact]
    public async Task EnsureAuthenticatedDoesNotThrowWhenAuthenticated() {
        using var plainMasterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

        var randomByteGenerator = new RandomByteGenerator();
        var hashProvider = new HashProvider();
        using var subject = new StoreManager(new Encryptor(randomByteGenerator), new StorageSiteFactory(), randomByteGenerator, hashProvider);

        using var expectedStoreKey = hashProvider.Provide(plainMasterPasswordBuffer, StoreManagerFactory.MasterPasswordSalt);

        await subject.Authenticate(plainMasterPasswordBuffer);

        using var result = await subject.GetPlainStoreKeyBuffer();

        Assert.Equal(expectedStoreKey.Value, result.Value);
    }

    [Fact]
    public async Task SaveStorageSitesAndLoadStorageSites() {
        using var masterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());

        using var saveSubject = await new StoreManagerFactory().Create(masterPasswordBuffer);
        saveSubject.StorageSites.Add(new FileSystemStorageSite("abc"u8));
        saveSubject.StorageSites.Add(new FileSystemStorageSite("def"u8));
        var encryptedSettingsBuffer = await saveSubject.SaveStorageSites();

        using var loadSubject = await new StoreManagerFactory().Create(masterPasswordBuffer);
        await loadSubject.LoadStorageSites(encryptedSettingsBuffer);

        Assert.Equal(2, loadSubject.StorageSites.Count);
        Assert.Equal("abc"u8, Assert.IsType<FileSystemStorageSite>(loadSubject.StorageSites[0]).Location);
        Assert.Equal("def"u8, Assert.IsType<FileSystemStorageSite>(loadSubject.StorageSites[1]).Location);
    }

    [Fact]
    public async Task GetPlainStoreKey() {
        using var masterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());
        using var expectedStoreKey = new HashProvider().Provide(masterPasswordBuffer, StoreManagerFactory.MasterPasswordSalt);

        using var subject = await new StoreManagerFactory().Create(masterPasswordBuffer);
        
        using var result = await subject.GetPlainStoreKeyBuffer();

        Assert.Equal(expectedStoreKey.Value, result.Value);
    }

    [Fact]
    public async Task Dispose() {
        SecureBuffer plainSessionKeyBuffer;
        SecureBuffer encryptedStoreKeyBuffer;

        using var masterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());
        using var expectedStoreKey = new HashProvider().Provide(masterPasswordBuffer, StoreManagerFactory.MasterPasswordSalt);

        using (var subject = await new StoreManagerFactory().Create(masterPasswordBuffer)) {
            plainSessionKeyBuffer = subject.GetBuffer("plainSessionKeyBuffer");
            encryptedStoreKeyBuffer = subject.GetBuffer("encryptedStoreKeyBuffer");
        };
        
        Assert.True(plainSessionKeyBuffer.IsDisposed);
        Assert.True(encryptedStoreKeyBuffer.IsDisposed);
    }
}
