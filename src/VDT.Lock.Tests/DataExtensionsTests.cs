using Xunit;

namespace VDT.Lock.Tests;

public class DataExtensionsTests {
    [Fact]
    public void GetLength() {
        var subject = new DataField([97, 98, 99], [1, 2, 3, 4, 5]);
        subject.Selectors.Add(new DataValue([1, 2, 3, 4, 5]));

        Assert.Equal(69, subject.GetLength());
    }
}
