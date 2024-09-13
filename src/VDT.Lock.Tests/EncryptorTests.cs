using NSubstitute;
using System.IO;
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
        var inputStream = "password".ToStream();
        var iv = new byte[Encryptor.KeySizeInBytes];

        var result = await subject.Encrypt(inputStream, iv);

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void Decrypt() {
        var expectedResult = "password"u8.ToArray();

        var randomByteGenerator = Substitute.For<IRandomByteGenerator>();
        randomByteGenerator.Generate(Arg.Any<int>()).Returns(callInfo => new byte[callInfo.ArgAt<int>(0)]);

        var subject = new Encryptor(randomByteGenerator);
        var encryptedBytes = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 152, 35, 44, 52, 128, 214, 112, 9, 199, 100, 68, 128, 204, 138, 16, 22 };

        var result = subject.Decrypt(encryptedBytes, new byte[Encryptor.KeySizeInBytes]);

        var memoryStream = new MemoryStream();
        result.CopyTo(memoryStream);

        Assert.Equal(expectedResult, memoryStream.ToArray());
    }
}
