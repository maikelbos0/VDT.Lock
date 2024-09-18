﻿using NSubstitute;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
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

        Assert.Equal(GetExpectedBuffer(SecureByteList.DefaultCapacity), GetBuffer(subject));
    }

    [Fact]
    public void StreamConstructor() {
        var stream = "abc".ToStream();

        using var subject = new SecureByteList(stream);

        Assert.Equal("abc"u8.ToArray(), subject.GetValue());
        Assert.Equal(GetExpectedBuffer(SecureByteList.DefaultCapacity * 2, "abc"u8.ToArray()), GetBuffer(subject));
    }

    [Fact]
    public void StreamConstructorThrowsForUnreadableStream() {
        var stream = Substitute.For<Stream>();
        stream.CanRead.Returns(false);

        Assert.Throws<ArgumentException>(() => new SecureByteList(stream));
    }

    [Fact]
    public void ByteArrayConstructor() {
        var byteArray = new byte[] { 97, 98, 99 };

        using var subject = new SecureByteList(byteArray);

        Assert.Equal(byteArray, subject.GetValue());
        Assert.Same(byteArray, GetBuffer(subject));
    }

    [Fact]
    public void AddChar() {
        using var subject = new SecureByteList();

        subject.Add('a');

        Assert.Equal("a"u8.ToArray(), subject.GetValue());
        Assert.Equal(GetExpectedBuffer(SecureByteList.DefaultCapacity, "a"u8.ToArray()), GetBuffer(subject));
    }

    [Fact]
    public void AddByte() {
        using var subject = new SecureByteList();

        subject.Add(97);

        Assert.Equal(new byte[] { 97 }, subject.GetValue());
        Assert.Equal(GetExpectedBuffer(SecureByteList.DefaultCapacity, 97), GetBuffer(subject));
    }

    [Fact]
    public void RemoveLast() {
        using var subject = new SecureByteList();

        subject.Add('a');
        subject.Add('b');
        subject.Add('c');

        subject.RemoveLast();

        Assert.Equal("ab"u8.ToArray(), subject.GetValue());
        Assert.Equal(GetExpectedBuffer(SecureByteList.DefaultCapacity, 97, 98), GetBuffer(subject));
    }

    [Fact]
    public void Clear() {
        using var subject = new SecureByteList();

        subject.Add('a');
        subject.Add('b');
        subject.Add('c');

        subject.Clear();

        Assert.Equal(Array.Empty<byte>(), subject.GetValue());
        Assert.Equal(GetExpectedBuffer(SecureByteList.DefaultCapacity), GetBuffer(subject));
    }

    [Fact]
    public void EnsureCapacity() {
        using var subject = new SecureByteList();
        var oldBuffer = GetBuffer(subject);

        subject.Add(97);
        subject.Add(98);
        subject.Add(99);

        subject.EnsureCapacity(SecureByteList.DefaultCapacity + 1);

        Assert.Equal(new byte[] { 97, 98, 99 }, subject.GetValue());
        Assert.Equal(GetExpectedBuffer(SecureByteList.DefaultCapacity), oldBuffer);
        Assert.Equal(GetExpectedBuffer(SecureByteList.DefaultCapacity * 2, 97, 98, 99), GetBuffer(subject));
    }

    [Fact]
    public void AddEnsuresCapacity() {
        using var subject = new SecureByteList();

        for (var i = 0; i < SecureByteList.DefaultCapacity + 1; i++) {
            subject.Add(97);
        }

        var expectedValue = Enumerable.Repeat((byte)97, SecureByteList.DefaultCapacity + 1).ToArray();

        Assert.Equal(expectedValue, subject.GetValue());
        Assert.Equal(GetExpectedBuffer(SecureByteList.DefaultCapacity * 2, expectedValue), GetBuffer(subject));
    }

    private static byte[] GetExpectedBuffer(int length, params byte[] bytes) {
        var expectedBuffer = new byte[length];
        Buffer.BlockCopy(bytes, 0, expectedBuffer, 0, bytes.Length);
        return expectedBuffer;
    }

    private static byte[] GetBuffer(SecureByteList list) {
        var fieldInfo = typeof(SecureByteList).GetField("buffer", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();
        var buffer = fieldInfo.GetValue(list) as SecureBuffer ?? throw new InvalidOperationException();

        return buffer.Value;
    }
}
