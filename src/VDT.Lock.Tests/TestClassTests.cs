using Xunit;

namespace VDT.Lock.Tests;

public class TestClassTests {
    [Fact]
    public void Test() {
        Assert.Equal("Hello world!", TestClass.Test());
    }
}
