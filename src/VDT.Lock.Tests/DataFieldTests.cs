using System;
using Xunit;

namespace VDT.Lock.Tests;

public class DataFieldTests {
    [Fact]
    public void DeserializingConstructor() {
        var plainSpan = new ReadOnlySpan<byte>([
            3, 0, 0, 0, 98, 97, 114,
            5, 0, 0, 0, 5, 6, 7, 8, 9
        ]);

        using var subject = new DataField(plainSpan);

        Assert.Equal(new ReadOnlySpan<byte>([98, 97, 114]), subject.Name);
        Assert.Equal(new ReadOnlySpan<byte>([5, 6, 7, 8, 9]), subject.Data);
    }

    [Fact]
    public void Constructor() {
        var plainNameSpan = new ReadOnlySpan<byte>([98, 97, 114]);
        var plainDataSpan = new ReadOnlySpan<byte>([5, 6, 7, 8, 9]);

        using var subject = new DataField([98, 97, 114], [5, 6, 7, 8, 9]);

        Assert.Equal(plainNameSpan, subject.Name);
        Assert.Equal(plainDataSpan, subject.Data);

    }

    [Fact]
    public void SetName() {
        using var subject = new DataField([98, 97, 114], [5, 6, 7, 8, 9]);

        var plainPreviousValueBuffer = subject.GetBuffer("plainNameBuffer");

        subject.Name = new ReadOnlySpan<byte>([99, 99, 99]);

        Assert.Equal(new ReadOnlySpan<byte>([99, 99, 99]), subject.Name);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }
    
    [Fact]
    public void SetData() {
        using var subject = new DataField([98, 97, 114], [5, 6, 7, 8, 9]);

        var plainPreviousValueBuffer = subject.GetBuffer("plainDataBuffer");

        subject.Data = new ReadOnlySpan<byte>([99, 99, 99]);

        Assert.Equal(new ReadOnlySpan<byte>([99, 99, 99]), subject.Data);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }

    [Fact]
    public void SerializeTo() {
        var plainSpan = new ReadOnlySpan<byte>([
            3, 0, 0, 0, 98, 97, 114,
            5, 0, 0, 0, 5, 6, 7, 8, 9
        ]);

        using var subject = new DataField(plainSpan);

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        var resultValue = result.GetValue();

        Assert.Equal(plainSpan.Length, resultValue[0]);
        Assert.Equal(plainSpan, resultValue.Slice(4));
    }        

    [Fact]
    public void Dispose() {
        var plainSpan = new ReadOnlySpan<byte>([
            3, 0, 0, 0, 98, 97, 114,
            5, 0, 0, 0, 5, 6, 7, 8, 9
        ]);
        SecureBuffer plainNameBuffer;
        SecureBuffer plainDataBuffer;

        using (var subject = new DataField(plainSpan)) {
            plainNameBuffer = subject.GetBuffer("plainNameBuffer");
            plainDataBuffer = subject.GetBuffer("plainDataBuffer");
        };

        Assert.True(plainNameBuffer.IsDisposed);
        Assert.True(plainDataBuffer.IsDisposed);
    }
}
