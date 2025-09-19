using System;
using Xunit;

namespace VDT.Lock.Tests;

public class SecureBufferSplitterTests {
    [Theory]
    [InlineData(2, new byte[] { 15, 0, 0, 0})]
    [InlineData(7, new byte[] { 5, 0, 0, 0})]
    [InlineData(14, new byte[] { 3, 0, 0, 0})]
    [InlineData(29, new byte[] { 1, 0, 0, 0})]
    [InlineData(30, new byte[] { 1, 0, 0, 0})]
    [InlineData(7 * 1024, new byte[] { 1, 0, 0, 0})]
    public void GetHeader(int sectionSize, byte[] expectedResult) {
        using var buffer = new SecureBuffer([83, 112, 108, 105, 116, 32, 109, 101, 32, 105, 110, 116, 111, 32, 115, 101, 99, 116, 105, 111, 110, 115, 32, 112, 108, 101, 97, 115, 101]);
        var subject = new SecureBufferSplitter(buffer, sectionSize);

        var result = subject.GetHeader();

        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(0, new byte[] { 83, 112, 108, 105, 116, 32, 109, 101 })]
    [InlineData(1, new byte[] { 32, 105, 110, 116, 111, 32, 115, 101 })]
    [InlineData(3, new byte[] { 108, 101, 97, 115, 101 })]
    public void GetSection(int sectionIndex, byte[] expectedResult) {
        using var buffer = new SecureBuffer([83, 112, 108, 105, 116, 32, 109, 101, 32, 105, 110, 116, 111, 32, 115, 101, 99, 116, 105, 111, 110, 115, 32, 112, 108, 101, 97, 115, 101]);
        var subject = new SecureBufferSplitter(buffer, 8);

        var result = subject.GetSection(sectionIndex);

        Assert.Equal(expectedResult, result);
    }
}
