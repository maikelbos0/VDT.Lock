using NSubstitute;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VDT.Lock.Tests;

public class EncryptorTests {
    [Fact]
    public async Task Encrypt() {
        var expectedResult = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 152, 35, 44, 52, 128, 214, 112, 9, 199, 100, 68, 128, 204, 138, 16, 22 };

        var randomByteGenerator = Substitute.For<IRandomByteGenerator>();
        randomByteGenerator.Generate(Arg.Any<int>()).Returns(callInfo => new byte[callInfo.ArgAt<int>(0)]);

        var subject = new Encryptor(randomByteGenerator);
        using var plainBuffer = new SecureBuffer("password"u8.ToArray());
        var key = new byte[Encryptor.KeySizeInBytes];

        var result = await subject.Encrypt(plainBuffer, key);

        Assert.Equal(expectedResult, result.Value);
    }

    [Fact]
    public async Task Decrypt() {
        var expectedResult = "password"u8.ToArray();

        var randomByteGenerator = Substitute.For<IRandomByteGenerator>();
        randomByteGenerator.Generate(Arg.Any<int>()).Returns(callInfo => new byte[callInfo.ArgAt<int>(0)]);

        var subject = new Encryptor(randomByteGenerator);
        var payloadBuffer = new SecureBuffer([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 152, 35, 44, 52, 128, 214, 112, 9, 199, 100, 68, 128, 204, 138, 16, 22]);
        var key = new byte[Encryptor.KeySizeInBytes];

        var result = await subject.Decrypt(payloadBuffer, key);

        Assert.Equal(expectedResult, result.Value);
    }

    [Fact]
    public async Task RoundTrip() {
        var expectedResult = new SecureBuffer(Encoding.UTF8.GetBytes("password"));

        var randomByteGenerator = new RandomByteGenerator();

        var subject = new Encryptor(randomByteGenerator);
        var key = randomByteGenerator.Generate(Encryptor.KeySizeInBytes);

        var encryptedBytes = await subject.Encrypt(expectedResult, key);
        var result = await subject.Decrypt(encryptedBytes, key);

        Assert.Equal(expectedResult.Value, result.Value);
    }
}
