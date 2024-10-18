using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VDT.Lock.Tests;

public class EncryptorTests {
    [Fact]
    public async Task EncryptAndDecrypt() {
        var expectedResult = Encoding.UTF8.GetBytes("password");

        var randomByteGenerator = new RandomByteGenerator();
        var subject = new Encryptor(randomByteGenerator);

        using var keyBuffer = new SecureBuffer(randomByteGenerator.Generate(Encryptor.KeySizeInBytes));
        using var plainBuffer = new SecureBuffer(expectedResult);
        var encryptedBytes = await subject.Encrypt(plainBuffer, keyBuffer);
        var result = await subject.Decrypt(encryptedBytes, keyBuffer);

        Assert.Equal(expectedResult, result.Value);
    }
}
