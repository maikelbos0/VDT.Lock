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

        var stream = new MemoryStream();
        var password = "password"u8.ToArray();
        stream.Write(password, 0, password.Length);
        stream.Seek(0, SeekOrigin.Begin);

        var result = await subject.Encrypt(stream, new byte[Encryptor.KeySizeInBytes]);

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void Decrypt() {
        var expectedResult = "password"u8.ToArray(); 
        
        var randomByteGenerator = Substitute.For<IRandomByteGenerator>();
        randomByteGenerator.Generate(Arg.Any<int>()).Returns(callInfo => new byte[callInfo.ArgAt<int>(0)]);

        var bytes = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 152, 35, 44, 52, 128, 214, 112, 9, 199, 100, 68, 128, 204, 138, 16, 22 };

        var subject = new Encryptor(randomByteGenerator);
                
        var result = subject.Decrypt(bytes, new byte[Encryptor.KeySizeInBytes]);

        var memoryStream = new MemoryStream();
        result.CopyTo(memoryStream);

        Assert.Equal(expectedResult, memoryStream.ToArray());
    }
}
