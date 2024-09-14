﻿using System;
using System.Reflection;
using Xunit;

namespace VDT.Lock.Tests;

public sealed class SecureByteArrayTests {
    [Fact]
    public void PushChar() {
        using var subject = new SecureByteArray(4);

        subject.Push('a');

        Assert.Equal("a"u8.ToArray(), subject.GetValue());
        Assert.Equal(new byte[] { (byte)'a', 0, 0, 0 } , GetBuffer(subject));
    }

    [Fact]
    public void PushByte() {
        using var subject = new SecureByteArray(4);

        subject.Push(97);

        Assert.Equal("a"u8.ToArray(), subject.GetValue());
        Assert.Equal(new byte[] { 97, 0, 0, 0 }, GetBuffer(subject));
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
