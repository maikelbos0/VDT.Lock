using Xunit;

namespace VDT.Lock.Tests;

public class DataExtensionsTests {
    [Fact]
    public void GetLength() {
        var dataField = new DataField([97, 98, 99], [1, 2, 3, 4, 5]);

        Assert.Equal(16, dataField.GetLength());
    }
}
