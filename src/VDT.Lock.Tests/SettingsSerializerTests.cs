﻿using System;
using Xunit;

namespace VDT.Lock.Tests;

public class SettingsSerializerTests {
    [Fact]
    public void ReadInt() {
        var plainSettingsSpan = new ReadOnlySpan<byte>([0, 0, 0, 0, 8, 4, 2, 1, 0, 0, 0, 0]);
        var position = 4;

        var result = SettingsSerializer.ReadInt(plainSettingsSpan, ref position);

        Assert.Equal(8, position);
        Assert.Equal(16909320, result);
    }

    [Fact]
    public void ReadSpan() {
        var plainSettingsSpan = new ReadOnlySpan<byte>([0, 0, 0, 0, 3, 0, 0, 0, 97, 98, 99, 0]);
        var position = 4;

        var result = SettingsSerializer.ReadSpan(plainSettingsSpan, ref position);

        Assert.Equal(11, position);
        Assert.Equal("abc"u8, result);
    }

    [Fact]
    public void ReadString() {
        var plainSettingsSpan = new ReadOnlySpan<byte>([0, 0, 0, 0, 3, 0, 0, 0, 97, 98, 99, 0]);
        var position = 4;

        var result = SettingsSerializer.ReadString(plainSettingsSpan, ref position);

        Assert.Equal(11, position);
        Assert.Equal("abc", result);
    }

    [Fact]
    public void ReadSecureBuffer() {
        var plainSettingsSpan = new ReadOnlySpan<byte>([0, 0, 0, 0, 3, 0, 0, 0, 97, 98, 99, 0]);
        var position = 4;

        using var result = SettingsSerializer.ReadSecureBuffer(plainSettingsSpan, ref position);

        Assert.Equal(11, position);
        Assert.Equal(new byte[] { 97, 98, 99 }, result.Value);
    }

    [Fact]
    public void WriteInt() {
        using var plainSettingsBytes = new SecureByteList();

        SettingsSerializer.WriteInt(plainSettingsBytes, 16909320);

        Assert.Equal(new byte[] { 8, 4, 2, 1 }, plainSettingsBytes.GetValue());
    }

    [Fact]
    public void WriteString() {
        using var plainSettingsBytes = new SecureByteList();

        SettingsSerializer.WriteString(plainSettingsBytes, "abc");

        Assert.Equal(new byte[] { 3, 0, 0, 0, 97, 98, 99 }, plainSettingsBytes.GetValue());
    }

    [Fact]
    public void WriteSecureBuffer() {
        using var plainSettingsBytes = new SecureByteList();
        using var plainBuffer = new SecureBuffer(new byte[] { 97, 98, 99 });

        SettingsSerializer.WriteSecureBuffer(plainSettingsBytes, plainBuffer);

        Assert.Equal(new byte[] { 3, 0, 0, 0, 97, 98, 99 }, plainSettingsBytes.GetValue());
    }
}
