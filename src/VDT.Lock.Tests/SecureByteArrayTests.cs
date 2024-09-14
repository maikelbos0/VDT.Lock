using System;
using System.Reflection;
using Xunit;

namespace VDT.Lock.Tests;

public sealed class SecureByteArrayTests {
    [Fact]
    public void AddChar() {
        using var subject = new SecureByteArray(4);

        subject.Add('a');

        Assert.Equal("a"u8.ToArray(), subject.GetValue());
        Assert.Equal(new byte[] { (byte)'a', 0, 0, 0 } , GetBuffer(subject));
    }

    [Fact]
    public void AddByte() {
        using var subject = new SecureByteArray(4);

        subject.Add(97);

        Assert.Equal(new byte[] { 97 }, subject.GetValue());
        Assert.Equal(new byte[] { 97, 0, 0, 0 }, GetBuffer(subject));
    }

    [Fact]
    public void Clear() {
        using var subject = new SecureByteArray(4);

        subject.Add('a');
        subject.Add('b');
        subject.Add('c');

        subject.Clear();

        Assert.Equal(Array.Empty<byte>(), subject.GetValue());
        Assert.Equal(new byte[4], GetBuffer(subject));
    }

    private static byte[] GetBuffer(SecureByteArray bytes) {
        var fieldInfo = typeof(SecureByteArray).GetField("buffer", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException();

        return fieldInfo.GetValue(bytes) as byte[]
            ?? throw new InvalidOperationException();
    }
}
