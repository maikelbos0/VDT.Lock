using Xunit;

namespace VDT.Lock.Tests;

public class SecureBufferTests {
    [Fact]
    public void Dispose() {
        byte[] bufferValue = [97, 98, 99];

        using (var subject = new SecureBuffer(bufferValue)) { }

        Assert.Equal([0, 0, 0], bufferValue);
    }
}
