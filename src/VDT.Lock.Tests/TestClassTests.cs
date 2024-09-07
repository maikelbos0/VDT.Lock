namespace VDT.Lock.Tests;

public class TestClassTests {
    [Fact]
    public void Test() {
        var subject = new TestClass();

        Assert.True(subject.Test());
    }
}
