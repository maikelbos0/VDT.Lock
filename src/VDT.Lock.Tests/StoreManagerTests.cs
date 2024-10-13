﻿using System;
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

        using var expectedStoreKey = hashProvider.Provide(plainMasterPasswordBuffer, StoreManager.MasterPasswordSalt);

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
