using System;
using System.Text;
using System.Threading.Tasks;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests.StorageSites;

public class FileSystemStorageSiteTests {
    [Fact]
    public void Constructor() {
        using var subject = new FileSystemStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), new ReadOnlySpan<byte>([97, 98, 99]));

        Assert.Equal(new byte[] { 102, 111, 111 }, subject.Name);
        Assert.Equal(new byte[] { 97, 98, 99 }, subject.Location);
    }

    [Fact]
    public async Task ExecuteLoad() {
        const string fileName = "FileSystemStorage_ExecuteLoad.data";
        var expectedResult = ContentProvider.GetFileContents(fileName);

        using var subject = new FileSystemStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), Encoding.UTF8.GetBytes(ContentProvider.GetFilePath(fileName)));

        var result = await subject.Load();

        Assert.NotNull(result);
        Assert.Equal(expectedResult, result.Value);
    }

    [Fact]
    public async Task ExecuteSave() {
        const string fileName = "FileSystemStorage_ExecuteSave.data";
        const string fileContents = "This is not actually encrypted data, but normally it would be.";
        var expectedResult = Encoding.UTF8.GetBytes(fileContents);

        using var subject = new FileSystemStorageSite(new ReadOnlySpan<byte>([102, 111, 111]), Encoding.UTF8.GetBytes(ContentProvider.GetFilePath(fileName)));

        Assert.True(await subject.Save(new SecureBuffer(expectedResult)));

        var result = ContentProvider.GetFileContents(fileName);

        Assert.Equal(expectedResult, result);
    }
}
