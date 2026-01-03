using System;
using System.Collections.Generic;
using Xunit;

namespace VDT.Lock.Tests;

public class DataExtensionsTests {
    private sealed class Data : IData<Data> {
        public required IEnumerable<int> FieldLengths { get; set; }

        public static Data DeserializeFrom(ReadOnlySpan<byte> plainSpan) => throw new NotImplementedException();

        public void SerializeTo(SecureByteList plainBytes) => throw new NotImplementedException();
    }

    [Fact]
    public void Length() {
        var subject = new Data() {
            FieldLengths = [2, 3, 5, 7, 11]
        };

        Assert.Equal(48, subject.Length);
    }
}
