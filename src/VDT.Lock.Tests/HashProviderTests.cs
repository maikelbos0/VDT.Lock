using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace VDT.Lock.Tests;

public class HashProviderTests {
    [Fact]
    public void Provide() {
        var subject = new HashProvider();

        using var plainBuffer = new SecureBuffer(Encoding.UTF8.GetBytes("password"));
        var salt = new byte[] { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 };
        var result = subject.Provide(plainBuffer, salt);
        
        var expectedResult = Rfc2898DeriveBytes.Pbkdf2(plainBuffer.Value, salt, HashProvider.Iterations, HashAlgorithmName.SHA256, Encryptor.KeySizeInBytes);

        Assert.Equal(expectedResult, result.Value);
    }
}
