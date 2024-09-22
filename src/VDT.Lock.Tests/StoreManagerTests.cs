using System.Threading.Tasks;
using Xunit;

namespace VDT.Lock.Tests;

public class StoreManagerTests {
    [Fact]
    public async Task GetStoreKey() {
        using var masterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());
        using var expectedStoreKey = new HashProvider().Provide(masterPasswordBuffer, StoreManagerFactory.MasterPasswordSalt);

        using var subject = await new StoreManagerFactory().Create(masterPasswordBuffer);
        
        using var result = await subject.GetStoreKey();

        Assert.Equal(expectedStoreKey.Value, result.Value);
    }

    [Fact]
    public async Task Dispose() {
        byte[] sessionKeyBufferValue;
        byte[] encryptedStoreKeyBufferValue;

        using var masterPasswordBuffer = new SecureBuffer("aVerySecurePassword"u8.ToArray());
        using var expectedStoreKey = new HashProvider().Provide(masterPasswordBuffer, StoreManagerFactory.MasterPasswordSalt);

        using (var subject = await new StoreManagerFactory().Create(masterPasswordBuffer)) {
            sessionKeyBufferValue = subject.GetBufferValue("sessionKeyBuffer");
            encryptedStoreKeyBufferValue = subject.GetBufferValue("encryptedStoreKeyBuffer");
        };
        
        Assert.Equal(new byte[Encryptor.KeySizeInBytes], sessionKeyBufferValue);
        Assert.Equal(new byte[Encryptor.BlockSizeInBytes + Encryptor.BlockSizeInBytes + Encryptor.KeySizeInBytes], encryptedStoreKeyBufferValue);
    }
}
