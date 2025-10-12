using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VDT.Lock.Services;
using Xunit;

namespace VDT.Lock.Tests.Services;

public class EncryptorTests {
    [Fact]
    public async Task EncryptAndDecrypt() {
        var expectedResult = Encoding.UTF8.GetBytes("password");

        var subject = new Encryptor();

        using var keyBuffer = new SecureBuffer(RandomNumberGenerator.GetBytes(Encryptor.KeySizeInBytes));
        using var plainBuffer = new SecureBuffer(expectedResult);
        var encryptedBytes = await subject.Encrypt(plainBuffer, keyBuffer);
        var result = await subject.Decrypt(encryptedBytes, keyBuffer);

        Assert.Equal(expectedResult, result.Value);
    }
}
