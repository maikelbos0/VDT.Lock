using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests;

public class StorageSiteBaseTests {
    public class TestStorageSite : StorageSiteBase {
        public TestStorageSite(ReadOnlySpan<byte> plainNameSpan) : base(plainNameSpan) { }

        public override IEnumerable<int> FieldLengths => throw new NotImplementedException();

        public override void SerializeTo(SecureByteList plainBytes) {
            throw new NotImplementedException();
        }

        protected override Task<SecureBuffer?> ExecuteLoad()
            => Task.FromResult<SecureBuffer?>(new SecureBuffer([]));

        protected override Task<bool> ExecuteSave(SecureBuffer encryptedBuffer)
            => Task.FromResult(true);
    }

    [Fact]
    public void DeserializeFromCreatesChromeStorageSite() {
        var result = StorageSiteBase.DeserializeFrom(new ReadOnlySpan<byte>([0, 0, 0, 0, 0, 0, 0, 0]));

        Assert.IsType<ChromeStorageSite>(result);
    }

    [Fact]
    public void DeserializeFromCreatesFileSystemStorageSite() {
        var result = StorageSiteBase.DeserializeFrom(new ReadOnlySpan<byte>([1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]));

        Assert.IsType<FileSystemStorageSite>(result);
    }

    [Fact]
    public void SetName() {
        using var subject = new TestStorageSite([]);

        var plainPreviousValueBuffer = subject.GetBuffer<StorageSiteBase>("plainNameBuffer");

        subject.Name = new ReadOnlySpan<byte>([110, 97, 109, 101]);

        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), subject.Name);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }

    [Fact]
    public async Task Load() {
        using var subject = new TestStorageSite([]);

        var result = await subject.Load();

        Assert.NotNull(result);
        Assert.Equal([], result.Value);
    }

    [Fact]
    public async Task Save() {
        using var subject = new TestStorageSite([]);

        var result = await subject.Save(new SecureBuffer([]));

        Assert.True(result);
    }

    [Fact]
    public void Dispose() {
        TestStorageSite subject;
        SecureBuffer plainNameBuffer;

        using (subject = new([])) {
            plainNameBuffer = subject.GetBuffer<StorageSiteBase>("plainNameBuffer");
        }

        Assert.True(subject.IsDisposed);
        Assert.True(plainNameBuffer.IsDisposed);
    }

    [Fact]
    public void GetNameThrowsIfDisposed() {
        TestStorageSite subject;

        using (subject = new([])) { }

        Assert.Throws<ObjectDisposedException>(() => { var _ = subject.Name; });
    }

    [Fact]
    public void SetNameThrowsIfDisposed() {
        TestStorageSite subject;

        using (subject = new([])) { }

        Assert.Throws<ObjectDisposedException>(() => subject.Name = new ReadOnlySpan<byte>([110, 97, 109, 101]));
    }

    [Fact]
    public async Task LoadThrowsIfDisposed() {
        TestStorageSite subject;

        using (subject = new([])) { }

        await Assert.ThrowsAsync<ObjectDisposedException>(() => subject.Load());
    }

    [Fact]
    public async Task SaveThrowsIfDisposed() {
        TestStorageSite subject;

        using (subject = new([])) { }

        await Assert.ThrowsAsync<ObjectDisposedException>(() => subject.Save(new SecureBuffer([])));
    }
}
