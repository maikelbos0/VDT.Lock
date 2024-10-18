using System.Text;
using Xunit;

namespace VDT.Lock.Tests;

public class HashProviderTests {
    [Fact]
    public void Provide() {
        var expectedResult = new byte[] { 42, 3, 50, 180, 96, 215, 180, 143, 204, 234, 68, 211, 146, 144, 213, 1, 146, 63, 163, 109, 156, 255, 82, 157, 65, 255, 159, 145, 220, 11, 80, 75 };

        var subject = new HashProvider();

        using var plainBuffer = new SecureBuffer(Encoding.UTF8.GetBytes("password"));
        var salt = new byte[] { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 };
        var result = subject.Provide(plainBuffer, salt);

        Assert.Equal(expectedResult, result.Value);
    }
}
