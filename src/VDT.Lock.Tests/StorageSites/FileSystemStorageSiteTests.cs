using System;
using System.Text;
using System.Threading.Tasks;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests.StorageSites;

public class FileSystemStorageSiteTests {
    [Fact]
    public void DeserializeFrom() {
        var result = FileSystemStorageSite.DeserializeFrom(new ReadOnlySpan<byte>([4, 0, 0, 0, 110, 97, 109, 101, 8, 0, 0, 0, 108, 111, 99, 97, 116, 105, 111, 110]));

        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), result.Name);
        Assert.Equal(new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]), result.Location);
    }
    
    [Fact]
    public void Constructor() {
        using var subject = new FileSystemStorageSite(new ReadOnlySpan<byte>([110, 97, 109, 101]), new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]));

        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), subject.Name);
        Assert.Equal(new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]), subject.Location);
    }

    [Fact]
    public void FieldLengths() {
        using var subject = new FileSystemStorageSite(new ReadOnlySpan<byte>([110, 97, 109, 101]), new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]));

        Assert.Equal([0, 4, 8], subject.FieldLengths);
    }

    [Fact]
    public async Task ExecuteLoad() {
        const string fileName = "FileSystemStorage_ExecuteLoad.data";
        var expectedResult = ContentProvider.GetFileContents(fileName);

        using var subject = new FileSystemStorageSite(new ReadOnlySpan<byte>([110, 97, 109, 101]), Encoding.UTF8.GetBytes(ContentProvider.GetFilePath(fileName)));

        var result = await subject.Load();

        Assert.NotNull(result);
        Assert.Equal(expectedResult, result.Value);
    }

    [Fact]
    public async Task ExecuteSave() {
        const string fileName = "FileSystemStorage_ExecuteSave.data";
        const string fileContents = "This is not actually encrypted data, but normally it would be.";
        var expectedResult = Encoding.UTF8.GetBytes(fileContents);

        using var subject = new FileSystemStorageSite(new ReadOnlySpan<byte>([110, 97, 109, 101]), Encoding.UTF8.GetBytes(ContentProvider.GetFilePath(fileName)));

        Assert.True(await subject.Save(new SecureBuffer(expectedResult)));

        var result = ContentProvider.GetFileContents(fileName);

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new FileSystemStorageSite(new ReadOnlySpan<byte>([110, 97, 109, 101]), new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]));

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([24, 0, 0, 0, 1, 0, 0, 0, 4, 0, 0, 0, 110, 97, 109, 101, 8, 0, 0, 0, 108, 111, 99, 97, 116, 105, 111, 110]), result.GetValue());
    }

    [Fact]
    public void Dispose() {
        FileSystemStorageSite subject;
        SecureBuffer plainNameBuffer;
        SecureBuffer plainLocationBuffer;

        using (subject = new([], [])) {
            plainNameBuffer = subject.GetBuffer<StorageSiteBase>("plainNameBuffer");
            plainLocationBuffer = subject.GetBuffer("plainLocationBuffer");
        }

        Assert.True(subject.IsDisposed);
        Assert.True(plainNameBuffer.IsDisposed);
        Assert.True(plainLocationBuffer.IsDisposed);
    }

    [Fact]
    public void GetLocationThrowsIfDisposed() {
        FileSystemStorageSite subject;

        using (subject = new([], [])) { }

        Assert.Throws<ObjectDisposedException>(() => { var _ = subject.Location; });
    }

    [Fact]
    public void SetLocationThrowsIfDisposed() {
        FileSystemStorageSite subject;

        using (subject = new([], [])) { }

        Assert.Throws<ObjectDisposedException>(() => subject.Location = new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]));
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        FileSystemStorageSite subject;
        using var plainBytes = new SecureByteList();

        using (subject = new([], [])) { }

        Assert.Throws<ObjectDisposedException>(() => subject.SerializeTo(plainBytes));
    }
}
