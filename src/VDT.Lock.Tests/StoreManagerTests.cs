using System.Threading.Tasks;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests;

public class StoreManagerTests {
    [Fact]
    public async Task LoadStorageSites() {
        using var masterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());
        using var expectedStoreKey = new HashProvider().Provide(masterPasswordBuffer, StoreManagerFactory.MasterPasswordSalt);
        using var encryptedBuffer = new SecureBuffer([55, 29, 163, 61, 197, 212, 104, 99, 192, 148, 41, 57, 251, 55, 47, 229, 98, 64, 110, 12, 58, 1, 42, 214, 150, 102, 254, 85, 9, 36, 53, 226, 89, 199, 5, 205, 225, 99, 124, 67, 174, 56, 163, 156, 89, 139, 184, 0, 87, 93, 5, 12, 10, 139, 181, 144, 141, 177, 81, 37, 233, 146, 45, 0, 171, 78, 133, 94, 56, 29, 48, 98, 55, 10, 248, 194, 84, 84, 122, 217, 232, 102, 154, 252, 45, 231, 80, 200, 246, 59, 145, 199, 231, 253, 114, 33, 77, 148, 52, 233, 109, 114, 234, 92, 188, 21, 111, 104, 64, 212, 53, 182, 172, 144, 14, 34, 126, 4, 232, 233, 227, 80, 218, 90, 3, 150, 80, 4]);

        using var subject = await new StoreManagerFactory().Create(masterPasswordBuffer);

        await subject.LoadStorageSites(encryptedBuffer);

        Assert.Equal(2, subject.StorageSites.Count);
        Assert.Equal("abc"u8, Assert.IsType<FileSystemStorageSite>(subject.StorageSites[0]).Location);
        Assert.Equal("def"u8, Assert.IsType<FileSystemStorageSite>(subject.StorageSites[1]).Location);
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
