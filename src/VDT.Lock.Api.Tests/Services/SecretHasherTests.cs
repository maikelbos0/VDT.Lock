using System.Security.Cryptography;
using VDT.Lock.Api.Services;
using Xunit;

namespace VDT.Lock.Api.Tests.Services;

public class SecretHasherTests {
    [Fact]
    public void HashSecret() {
        var secret = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };

        var subject = new SecretHasher();

        var (salt, hash) = subject.HashSecret(secret);

        var expectedHash = Rfc2898DeriveBytes.Pbkdf2(secret, salt, SecretHasher.Iterations, HashAlgorithmName.SHA512, SHA512.HashSizeInBytes);

        Assert.Equal(expectedHash, hash);
    }
}
