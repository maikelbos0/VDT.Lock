using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VDT.Lock.Tests;

public class EncryptorTests {
    [Fact]
    public async Task EncryptAndDecrypt() {
        using var expectedResult = new SecureBuffer(Encoding.UTF8.GetBytes("password"));

        var randomByteGenerator = new RandomByteGenerator();

        var subject = new Encryptor(randomByteGenerator);
        using var keyBuffer = new SecureBuffer(randomByteGenerator.Generate(Encryptor.KeySizeInBytes));

        var encryptedBytes = await subject.Encrypt(expectedResult, keyBuffer);
        var result = await subject.Decrypt(encryptedBytes, keyBuffer);

        Assert.Equal(expectedResult.Value, result.Value);
    }
}
