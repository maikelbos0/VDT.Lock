using Xunit;

namespace VDT.Lock.Tests;

public class HashProviderTests {
    [Fact]
    public void Provide() {
        var expectedResult = new byte[] { 94, 127, 205, 144, 246, 91, 11, 57, 225, 92, 158, 62, 250, 193, 91, 110, 190, 217, 203, 200, 94, 121, 41, 154, 149, 62, 49, 26, 113, 1, 233, 151 };

        var subject = new HashProvider();
        var inputStream = "password".ToStream();
        var salt = new byte[] { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 };

        var result = subject.Provide(inputStream, salt);

        Assert.Equal(expectedResult, result);
    }
}
