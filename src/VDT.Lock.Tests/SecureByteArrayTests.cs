using NSubstitute;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace VDT.Lock.Tests;

public sealed class SecureByteArrayTests {
    [Fact]
    public void EmptyConstructor() {
        using var subject = new SecureByteArray(4);

        Assert.Equal(new byte[4], GetBuffer(subject));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void EmptyConstructorThrowsForZeroOrNegativeCapacity(int capacity) {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SecureByteArray(capacity));
    }

    [Fact]
    public void StreamConstructor() {
        var stream = "abc".ToStream();

        using var subject = new SecureByteArray(stream, 4);

        Assert.Equal("abc"u8.ToArray(), subject.GetValue());
        Assert.Equal(new byte[] { (byte)'a', (byte)'b', (byte)'c', 0, 0, 0, 0, 0 }, GetBuffer(subject));
    }

    [Fact]
    public void StreamConstructorThrowsForUnreadableStream() {
        var stream = Substitute.For<Stream>();
        stream.CanRead.Returns(false);

        Assert.Throws<ArgumentException>(() => new SecureByteArray(stream));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void StreamConstructorThrowsForZeroOrNegativeCapacity(int capacity) {
        var stream = "abc".ToStream();

        Assert.Throws<ArgumentOutOfRangeException>(() => new SecureByteArray(stream, capacity));
    }

    [Fact]
    public void PushChar() {
        using var subject = new SecureByteArray(4);

        subject.Push('a');

        Assert.Equal("a"u8.ToArray(), subject.GetValue());
        Assert.Equal(new byte[] { (byte)'a', 0, 0, 0 }, GetBuffer(subject));
    }

    [Fact]
    public void PushByte() {
        using var subject = new SecureByteArray(4);

        subject.Push(97);

        Assert.Equal("a"u8.ToArray(), subject.GetValue());
        Assert.Equal(new byte[] { 97, 0, 0, 0 }, GetBuffer(subject));
    }

    [Fact]
    public void EnsureCapacity() {
        using var subject = new SecureByteArray(4);
        var oldBuffer = GetBuffer(subject);

        subject.Push(97);
        subject.Push(98);
        subject.Push(99);

        subject.EnsureCapacity(6);

        Assert.Equal("abc"u8.ToArray(), subject.GetValue());
        Assert.Equal(new byte[4], oldBuffer);
        Assert.Equal(new byte[] { 97, 98, 99, 0, 0, 0, 0, 0 }, GetBuffer(subject));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void PushEnsuresCapacity(int capacity) {
        using var subject = new SecureByteArray(capacity);

        subject.Push(97);
        subject.Push(98);
        subject.Push(99);

        Assert.Equal("abc"u8.ToArray(), subject.GetValue());
        Assert.Equal(new byte[] { 97, 98, 99, 0 }, GetBuffer(subject));
    }

    [Fact]
    public void Pop() {
        using var subject = new SecureByteArray(4);

        subject.Push('a');
        subject.Push('b');
        subject.Push('c');

        subject.Pop();

        Assert.Equal("ab"u8.ToArray(), subject.GetValue());
        Assert.Equal(new byte[] { 97, 98, 0, 0 }, GetBuffer(subject));
    }

    [Fact]
    public void Clear() {
        using var subject = new SecureByteArray(4);

        subject.Push('a');
        subject.Push('b');
        subject.Push('c');

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
