using System;
using System.IO;
using System.Linq;
using Xunit;

namespace VDT.Lock.Tests;

public sealed class SecureByteListTests {
    [Theory]
    [InlineData(2, SecureByteList.DefaultCapacity)]
    [InlineData(SecureByteList.DefaultCapacity, SecureByteList.DefaultCapacity)]
    [InlineData(SecureByteList.DefaultCapacity + 1, SecureByteList.DefaultCapacity * 2)]
    [InlineData(SecureByteList.DefaultCapacity * 2 + 1, SecureByteList.DefaultCapacity * 4)]
    public void GetCapacity(int requestedCapacity, int expectedCapacity) {
        Assert.Equal(expectedCapacity, SecureByteList.GetCapacity(requestedCapacity));
    }

    [Fact]
    public void EmptyConstructor() {
        using var subject = new SecureByteList();

        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity), subject.GetBufferValue());
    }

    [Fact]
    public void StreamConstructor() {
        using var stream = new MemoryStream();
        stream.Write("abc"u8.ToArray(), 0, 3);
        stream.Seek(0, SeekOrigin.Begin);

        using var subject = new SecureByteList(stream);

        Assert.Equal("abc"u8.ToArray(), subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity * 2, "abc"u8.ToArray()), subject.GetBufferValue());
    }

    [Fact]
    public void ByteArrayConstructor() {
        var byteArray = new byte[] { 97, 98, 99 };

        using var subject = new SecureByteList(byteArray);

        Assert.Equal(byteArray, subject.GetValue());
        Assert.Same(byteArray, subject.GetBufferValue());
    }

    [Fact]
    public void AddChar() {
        using var subject = new SecureByteList();

        subject.Add('a');

        Assert.Equal("a"u8.ToArray(), subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity, "a"u8.ToArray()), subject.GetBufferValue());
    }

    [Fact]
    public void AddByte() {
        using var subject = new SecureByteList();

        subject.Add(97);

        Assert.Equal(new byte[] { 97 }, subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity, 97), subject.GetBufferValue());
    }

    [Fact]
    public void RemoveLast() {
        using var subject = new SecureByteList();

        subject.Add('a');
        subject.Add('b');
        subject.Add('c');

        subject.RemoveLast();

        Assert.Equal("ab"u8.ToArray(), subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity, 97, 98), subject.GetBufferValue());
    }

    [Fact]
    public void Clear() {
        using var subject = new SecureByteList();

        subject.Add('a');
        subject.Add('b');
        subject.Add('c');

        subject.Clear();

        Assert.Equal(Array.Empty<byte>(), subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity), subject.GetBufferValue());
    }

    [Fact]
    public void EnsureCapacity() {
        using var subject = new SecureByteList();
        var oldBuffer = subject.GetBufferValue();

        subject.Add(97);
        subject.Add(98);
        subject.Add(99);

        subject.EnsureCapacity(SecureByteList.DefaultCapacity + 1);

        Assert.Equal(new byte[] { 97, 98, 99 }, subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity), oldBuffer);
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity * 2, 97, 98, 99), subject.GetBufferValue());
    }

    [Fact]
    public void AddEnsuresCapacity() {
        using var subject = new SecureByteList();

        for (var i = 0; i < SecureByteList.DefaultCapacity + 1; i++) {
            subject.Add(97);
        }

        var expectedValue = Enumerable.Repeat((byte)97, SecureByteList.DefaultCapacity + 1).ToArray();

        Assert.Equal(expectedValue, subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity * 2, expectedValue), subject.GetBufferValue());
    }

    [Fact]
    public void ToBuffer() {
        using var subject = new SecureByteList();

        subject.Add(97);
        subject.Add(98);
        subject.Add(99);

        using var buffer = subject.ToBuffer();

        Assert.Equal(new byte[] { 97, 98, 99 }, buffer.Value);
    }

    [Fact]
    public void Dispose() {
        byte[] bufferValue;

        using (var subject = new SecureByteList()) {
            subject.Add(97);
            subject.Add(98);
            subject.Add(99);

            bufferValue = subject.GetBufferValue();
        }

        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity), bufferValue);
    }

    private static byte[] GetExpectedBufferValue(int length, params byte[] bytes) {
        var expectedBuffer = new byte[length];
        Buffer.BlockCopy(bytes, 0, expectedBuffer, 0, bytes.Length);
        return expectedBuffer;
    }
}
