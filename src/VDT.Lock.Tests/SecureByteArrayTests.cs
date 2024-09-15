using NSubstitute;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace VDT.Lock.Tests;

public sealed class SecureByteArrayTests {
    [Fact]
    public void EmptyConstructor() {
        using var subject = new SecureByteArray();

        Assert.Equal(GetExpectedBuffer(SecureByteArray.DefaultCapacity), GetBuffer(subject));
    }

    [Fact]
    public void StreamConstructor() {
        var stream = "abc".ToStream();

        using var subject = new SecureByteArray(stream);

        Assert.Equal("abc"u8.ToArray(), subject.GetValue());
        Assert.Equal(GetExpectedBuffer(SecureByteArray.DefaultCapacity, "abc"u8.ToArray()), GetBuffer(subject));
    }

    [Fact]
    public void StreamConstructorThrowsForUnreadableStream() {
        var stream = Substitute.For<Stream>();
        stream.CanRead.Returns(false);

        Assert.Throws<ArgumentException>(() => new SecureByteArray(stream));
    }

    [Fact]
    public void PushChar() {
        using var subject = new SecureByteArray();

        subject.Push('a');

        Assert.Equal("a"u8.ToArray(), subject.GetValue());
        Assert.Equal(GetExpectedBuffer(SecureByteArray.DefaultCapacity, "a"u8.ToArray()), GetBuffer(subject));
    }

    [Fact]
    public void PushByte() {
        using var subject = new SecureByteArray();

        subject.Push(97);

        Assert.Equal(new byte[] { 97 }, subject.GetValue());
        Assert.Equal(GetExpectedBuffer(SecureByteArray.DefaultCapacity, 97), GetBuffer(subject));
    }

    [Fact]
    public void EnsureCapacity() {
        using var subject = new SecureByteArray();
        var oldBuffer = GetBuffer(subject);

        subject.Push(97);
        subject.Push(98);
        subject.Push(99);

        subject.EnsureCapacity(SecureByteArray.DefaultCapacity + 1);

        Assert.Equal(new byte[] { 97, 98, 99 }, subject.GetValue());
        Assert.Equal(GetExpectedBuffer(SecureByteArray.DefaultCapacity), oldBuffer);
        Assert.Equal(GetExpectedBuffer(SecureByteArray.DefaultCapacity * 2, 97, 98, 99), GetBuffer(subject));
    }

    [Fact]
    public void PushEnsuresCapacity() {
        using var subject = new SecureByteArray();

        for (var i = 0; i < SecureByteArray.DefaultCapacity + 1; i++) {
            subject.Push(97);
        }

        var expectedValue = Enumerable.Repeat((byte)97, SecureByteArray.DefaultCapacity + 1).ToArray();

        Assert.Equal(expectedValue, subject.GetValue());
        Assert.Equal(GetExpectedBuffer(SecureByteArray.DefaultCapacity * 2, expectedValue), GetBuffer(subject));
    }

    [Fact]
    public void Pop() {
        using var subject = new SecureByteArray();

        subject.Push('a');
        subject.Push('b');
        subject.Push('c');

        subject.Pop();

        Assert.Equal("ab"u8.ToArray(), subject.GetValue());
        Assert.Equal(GetExpectedBuffer(SecureByteArray.DefaultCapacity, 97, 98), GetBuffer(subject));
    }

    [Fact]
    public void Clear() {
        using var subject = new SecureByteArray();

        subject.Push('a');
        subject.Push('b');
        subject.Push('c');

        subject.Clear();

        Assert.Equal(Array.Empty<byte>(), subject.GetValue());
        Assert.Equal(GetExpectedBuffer(SecureByteArray.DefaultCapacity), GetBuffer(subject));
    }

    private static byte[] GetExpectedBuffer(int length, params byte[] bytes) {
        var expectedBuffer = new byte[length];
        Buffer.BlockCopy(bytes, 0, expectedBuffer, 0, bytes.Length);
        return expectedBuffer;
    }

    private static byte[] GetBuffer(SecureByteArray bytes) {
        var fieldInfo = typeof(SecureByteArray).GetField("buffer", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException();

        return fieldInfo.GetValue(bytes) as byte[]
            ?? throw new InvalidOperationException();
    }
}
