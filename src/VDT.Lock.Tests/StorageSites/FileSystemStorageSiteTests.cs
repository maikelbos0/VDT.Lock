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

        Assert.Equal(new byte[] { 102, 111, 111  }, subject.Name);
        Assert.Equal(new byte[] { 97, 98, 99 }, subject.Location);
    }

    [Fact]
    public async Task ExecuteLoad() {
        const string fileName = "FileSystemStorage_ExecuteLoad.data";
        
        using var subject = new FileSystemStorageSite(Encoding.UTF8.GetBytes(ContentProvider.GetFilePath(fileName)));

        var result = await subject.Load();
        var expectedResult = ContentProvider.GetFileContents(fileName);

        Assert.Equal(expectedResult, result.Value);
    }
}
