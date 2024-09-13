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
        var plainStream = "password".ToStream();
        var key = new byte[Encryptor.KeySizeInBytes];

        var result = await subject.Encrypt(plainStream, key);

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task Decrypt() {
        var expectedResult = "password"u8.ToArray();

        var randomByteGenerator = Substitute.For<IRandomByteGenerator>();
        randomByteGenerator.Generate(Arg.Any<int>()).Returns(callInfo => new byte[callInfo.ArgAt<int>(0)]);

        var subject = new Encryptor(randomByteGenerator);
        var encryptedBytes = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 152, 35, 44, 52, 128, 214, 112, 9, 199, 100, 68, 128, 204, 138, 16, 22 };
        var key = new byte[Encryptor.KeySizeInBytes];

        var result = await subject.Decrypt(encryptedBytes, key);

        Assert.Equal(expectedResult, result.ToArray());
    }

    [Fact]
    public async Task RoundTrip() {
        var expectedResult = Encoding.UTF8.GetBytes("password");

        var randomByteGenerator = new RandomByteGenerator();

        var subject = new Encryptor(randomByteGenerator);
        var plainStream = expectedResult.ToStream();
        var key = randomByteGenerator.Generate(Encryptor.KeySizeInBytes);

        var encryptedBytes = await subject.Encrypt(plainStream, key);
        var result = await subject.Decrypt(encryptedBytes, key);

        Assert.Equal(expectedResult, result.ToArray());
    }
}
