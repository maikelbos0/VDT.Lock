using NSubstitute;
using Xunit;

namespace VDT.Lock.Tests;

public class DataExtensionsTests {
    [Fact]
    public void GetLength() {
        var data = Substitute.For<IData>();

        data.FieldLengths.Returns([3, 4, 5, 6]);

        Assert.Equal(34, data.GetLength());
    }
}
