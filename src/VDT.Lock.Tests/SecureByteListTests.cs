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

        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity), subject.GetBuffer().Value);
    }

    [Fact]
    public void StreamConstructor() {
        using var stream = new MemoryStream();
        stream.Write("abc"u8.ToArray(), 0, 3);
        stream.Seek(0, SeekOrigin.Begin);

        using var subject = new SecureByteList(stream);

        Assert.Equal("abc"u8.ToArray(), subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity * 2, "abc"u8.ToArray()), subject.GetBuffer().Value);
    }

    [Fact]
    public void ByteArrayConstructor() {
        var byteArray = new byte[] { 97, 98, 99 };

        using var subject = new SecureByteList(byteArray);

        Assert.Equal(byteArray, subject.GetValue());
        Assert.Same(byteArray, subject.GetBuffer().Value);
    }

    [Fact]
    public void AddChar() {
        using var subject = new SecureByteList();

        subject.Add('a');
        subject.Add('b');
        subject.Add('c');

        Assert.Equal("abc"u8.ToArray(), subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity, "abc"u8.ToArray()), subject.GetBuffer().Value);
    }

    [Fact]
    public void AddByte() {
        using var subject = new SecureByteList();

        subject.Add(97);
        subject.Add(98);
        subject.Add(99);

        Assert.Equal(new byte[] { 97, 98, 99 }, subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity, 97, 98, 99), subject.GetBuffer().Value);
    }

    [Fact]
    public void AddReadOnlySpan() {
        using var subject = new SecureByteList();

        subject.Add(97);
        subject.Add(new ReadOnlySpan<byte>([98, 99]));

        Assert.Equal(new byte[] { 97, 98, 99 }, subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity, 97, 98, 99), subject.GetBuffer().Value);
    }

    [Fact]
    public void RemoveLast() {
        using var subject = new SecureByteList();

        subject.Add('a');
        subject.Add('b');
        subject.Add('c');

        subject.RemoveLast();

        Assert.Equal("ab"u8.ToArray(), subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity, 97, 98), subject.GetBuffer().Value);
    }

    [Fact]
    public void Clear() {
        using var subject = new SecureByteList();

        subject.Add('a');
        subject.Add('b');
        subject.Add('c');

        subject.Clear();

        Assert.Equal(Array.Empty<byte>(), subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity), subject.GetBuffer().Value);
    }

    [Fact]
    public void EnsureCapacity() {
        using var subject = new SecureByteList();
        var oldBuffer = subject.GetBuffer();

        subject.Add(97);
        subject.Add(98);
        subject.Add(99);

        subject.EnsureCapacity(SecureByteList.DefaultCapacity + 1);

        Assert.Equal(new byte[] { 97, 98, 99 }, subject.GetValue());
        Assert.True(oldBuffer.IsDisposed);
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity * 2, 97, 98, 99), subject.GetBuffer().Value);
    }

    [Fact]
    public void AddCharEnsuresCapacity() {
        using var subject = new SecureByteList();

        for (var i = 0; i < SecureByteList.DefaultCapacity + 1; i++) {
            subject.Add('a');
        }

        var expectedValue = Enumerable.Repeat((byte)'a', SecureByteList.DefaultCapacity + 1).ToArray();

        Assert.Equal(expectedValue, subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity * 2, expectedValue), subject.GetBuffer().Value);
    }

    [Fact]
    public void AddByteEnsuresCapacity() {
        using var subject = new SecureByteList();

        for (var i = 0; i < SecureByteList.DefaultCapacity + 1; i++) {
            subject.Add(97);
        }

        var expectedValue = Enumerable.Repeat((byte)97, SecureByteList.DefaultCapacity + 1).ToArray();

        Assert.Equal(expectedValue, subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity * 2, expectedValue), subject.GetBuffer().Value);
    }

    [Fact]
    public void AddReadOnlySpanEnsuresCapacity() {
        var expectedValue = Enumerable.Repeat((byte)97, SecureByteList.DefaultCapacity + 1).ToArray();
        using var subject = new SecureByteList();

        subject.Add(new ReadOnlySpan<byte>(expectedValue));

        Assert.Equal(expectedValue, subject.GetValue());
        Assert.Equal(GetExpectedBufferValue(SecureByteList.DefaultCapacity * 2, expectedValue), subject.GetBuffer().Value);
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
        SecureBuffer buffer;

        using (var subject = new SecureByteList()) {
            buffer = subject.GetBuffer();
        }

        Assert.True(buffer.IsDisposed);
    }

    [Fact]
    public void IsDisposed() {
        SecureByteList disposedSubject;

        using (var subject = new SecureByteList()) {
            disposedSubject = subject;
        };

        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void AddCharThrowsIfDisposed() {
        SecureByteList disposedSubject;

        using (var subject = new SecureByteList()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Add('a'));
    }

    [Fact]
    public void AddByteThrowsIfDisposed() {
        SecureByteList disposedSubject;

        using (var subject = new SecureByteList()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Add(15));
    }

    [Fact]
    public void AddReadOnlySpanThrowsIfDisposed() {
        SecureByteList disposedSubject;

        using (var subject = new SecureByteList()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Add(new ReadOnlySpan<byte>([15, 15, 15])));
    }

    [Fact]
    public void EnsureCapacityThrowsIfDisposed() {
        SecureByteList disposedSubject;

        using (var subject = new SecureByteList()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.EnsureCapacity(0));
    }

    [Fact]
    public void RemoveLastThrowsIfDisposed() {
        SecureByteList disposedSubject;

        using (var subject = new SecureByteList()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.RemoveLast());
    }

    [Fact]
    public void ClearThrowsIfDisposed() {
        SecureByteList disposedSubject;

        using (var subject = new SecureByteList()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Clear());
    }

    [Fact]
    public void GetValueThrowsIfDisposed() {
        SecureByteList disposedSubject;

        using (var subject = new SecureByteList()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.GetValue());
    }

    [Fact]
    public void ToBufferThrowsIfDisposed() {
        SecureByteList disposedSubject;

        using (var subject = new SecureByteList()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.ToBuffer());
    }

    private static byte[] GetExpectedBufferValue(int length, params byte[] bytes) {
        var expectedBuffer = new byte[length];
        Buffer.BlockCopy(bytes, 0, expectedBuffer, 0, bytes.Length);
        return expectedBuffer;
    }
}
