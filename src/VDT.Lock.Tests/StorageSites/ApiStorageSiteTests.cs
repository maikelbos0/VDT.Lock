using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using VDT.Lock.Services;
using VDT.Lock.StorageSites;
using Xunit;

namespace VDT.Lock.Tests.StorageSites;

public class ApiStorageSiteTests {
    [Fact]
    public void DeserializeFrom() {
        var result = ApiStorageSite.DeserializeFrom(new ReadOnlySpan<byte>([4, 0, 0, 0, 110, 97, 109, 101, 8, 0, 0, 0, 108, 111, 99, 97, 116, 105, 111, 110, 2, 0, 0, 0, 105, 100, 6, 0, 0, 0, 115, 101, 99, 114, 101, 116]));

        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), result.Name);
        Assert.Equal(new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]), result.Location);
        Assert.Equal(new ReadOnlySpan<byte>([105, 100]), result.DataStoreId);
        Assert.Equal(new ReadOnlySpan<byte>([115, 101, 99, 114, 101, 116]), result.Secret);
    }

    [Fact]
    public void SetLocation() {
        var subject = new ApiStorageSite([], []);
        var plainPreviousValueBuffer = subject.GetBuffer("plainLocationBuffer");

        subject.Location = new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]);

        Assert.Equal(new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]), subject.Location);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }

    [Fact]
    public void Constructor() {
        var plainNameSpan = new ReadOnlySpan<byte>([110, 97, 109, 101]);
        var plainLocationSpan = new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]);
        var plainDataStoreIdSpan = new ReadOnlySpan<byte>([105, 100]);
        var plainSecretSpan = new ReadOnlySpan<byte>([115, 101, 99, 114, 101, 116]);

        using var subject = new ApiStorageSite(plainNameSpan, plainLocationSpan, plainDataStoreIdSpan, plainSecretSpan);

        Assert.Equal(plainNameSpan, subject.Name);
        Assert.Equal(plainLocationSpan, subject.Location);
        Assert.Equal(plainDataStoreIdSpan, subject.DataStoreId);
        Assert.Equal(plainSecretSpan, subject.Secret);
    }

    [Fact]
    public void FieldLengths() {
        using var subject = new ApiStorageSite([110, 97, 109, 101], [108, 111, 99, 97, 116, 105, 111, 110], [105, 100], [115, 101, 99, 114, 101, 116]);

        Assert.Equal([0, 4, 8, 2, 6], subject.FieldLengths);
    }

    [Fact]
    public async Task ExecuteLoad() {
        var expectedResult = Encoding.UTF8.GetBytes("This is not actually encrypted data, but normally it would be.");

        using var subject = new ApiStorageSite([110, 97, 109, 101], [104, 116, 116, 112, 115, 58, 47, 47, 108, 111, 99, 97, 108, 104, 111, 115, 116, 47], [105, 100], [115, 101, 99, 114, 101, 116]);

        var storageSiteServices = Substitute.For<IStorageSiteServices>();
        storageSiteServices.HttpService.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new ByteArrayContent(expectedResult)
        });

        var result = await subject.Load(storageSiteServices);

        Assert.NotNull(result);
        Assert.Equal(expectedResult, result.Value);

        await storageSiteServices.HttpService.DidNotReceive().SendAsync(Arg.Is<HttpRequestMessage>(message => message.Method == HttpMethod.Post));
        await storageSiteServices.HttpService.Received(1).SendAsync(Arg.Is<HttpRequestMessage>(message
            => message.Method == HttpMethod.Get
            && message.RequestUri == new Uri("https://localhost/id")
            && message.Headers.Contains("Secret")
            && message.Headers.GetValues("Secret").SequenceEqual(new string[] { Convert.ToBase64String(new byte[] { 115, 101, 99, 114, 101, 116 }) })));
    }

    [Fact]
    public async Task ExecuteLoadReturnsNullForError() {
        using var subject = new ApiStorageSite([110, 97, 109, 101], [104, 116, 116, 112, 115, 58, 47, 47, 108, 111, 99, 97, 108, 104, 111, 115, 116, 47], [105, 100], [115, 101, 99, 114, 101, 116]);

        var storageSiteServices = Substitute.For<IStorageSiteServices>();
        storageSiteServices.HttpService.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(new HttpResponseMessage() {
            StatusCode = HttpStatusCode.BadRequest
        });

        var result = await subject.Load(storageSiteServices);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteLoadInitializesIfNeeded() {
        var expectedResult = Encoding.UTF8.GetBytes("This is not actually encrypted data, but normally it would be.");
        var expectedId = Guid.NewGuid().ToString();

        using var subject = new ApiStorageSite([110, 97, 109, 101], [104, 116, 116, 112, 115, 58, 47, 47, 108, 111, 99, 97, 108, 104, 111, 115, 116, 47]);

        var storageSiteServices = Substitute.For<IStorageSiteServices>();
        storageSiteServices.HttpService.SendAsync(Arg.Is<HttpRequestMessage>(message => message.Method == HttpMethod.Post)).Returns(new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes(expectedId))
        });
        storageSiteServices.HttpService.SendAsync(Arg.Is<HttpRequestMessage>(message => message.Method == HttpMethod.Get)).Returns(new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new ByteArrayContent(expectedResult)
        });

        var result = await subject.Load(storageSiteServices);

        Assert.NotNull(result);
        Assert.Equal(expectedResult, result.Value);
        Assert.NotEqual(0, subject.Secret.Length);
        Assert.Equal(expectedId, Encoding.UTF8.GetString(subject.DataStoreId));

        var secret = Convert.ToBase64String(subject.Secret);

        await storageSiteServices.HttpService.Received(1).SendAsync(Arg.Is<HttpRequestMessage>(message
            => message.Method == HttpMethod.Post
            && message.RequestUri == new Uri($"https://localhost/")
            && message.Headers.Contains("Secret")
            && message.Headers.GetValues("Secret").SequenceEqual(new string[] { secret })));
        await storageSiteServices.HttpService.Received(1).SendAsync(Arg.Is<HttpRequestMessage>(message
            => message.Method == HttpMethod.Get
            && message.RequestUri == new Uri($"https://localhost/{expectedId}")
            && message.Headers.Contains("Secret")
            && message.Headers.GetValues("Secret").SequenceEqual(new string[] { secret })));
    }

    [Fact]
    public async Task ExecuteLoadReturnsNullForInitializationError() {
        using var subject = new ApiStorageSite([110, 97, 109, 101], [104, 116, 116, 112, 115, 58, 47, 47, 108, 111, 99, 97, 108, 104, 111, 115, 116, 47]);

        var storageSiteServices = Substitute.For<IStorageSiteServices>();
        storageSiteServices.HttpService.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(new HttpResponseMessage() {
            StatusCode = HttpStatusCode.BadRequest
        });

        var result = await subject.Load(storageSiteServices);

        Assert.Null(result);

        await storageSiteServices.HttpService.DidNotReceive().SendAsync(Arg.Is<HttpRequestMessage>(message => message.Method == HttpMethod.Put));
    }

    [Fact]
    public async Task ExecuteSave() {
        var expectedResult = Encoding.UTF8.GetBytes("This is not actually encrypted data, but normally it would be.");

        using var subject = new ApiStorageSite([110, 97, 109, 101], [104, 116, 116, 112, 115, 58, 47, 47, 108, 111, 99, 97, 108, 104, 111, 115, 116, 47], [105, 100], [115, 101, 99, 114, 101, 116]);

        var storageSiteServices = Substitute.For<IStorageSiteServices>();
        storageSiteServices.HttpService.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new ByteArrayContent(expectedResult)
        });

        var result = await subject.Save(new(expectedResult), storageSiteServices);

        Assert.True(result);

        await storageSiteServices.HttpService.DidNotReceive().SendAsync(Arg.Is<HttpRequestMessage>(message => message.Method == HttpMethod.Post));
        await storageSiteServices.HttpService.Received(1).SendAsync(Arg.Is<HttpRequestMessage>(message
            => message.Method == HttpMethod.Put
            && message.RequestUri == new Uri("https://localhost/id")
            && message.Headers.Contains("Secret")
            && message.Headers.GetValues("Secret").SequenceEqual(new string[] { Convert.ToBase64String(new byte[] { 115, 101, 99, 114, 101, 116 }) })
            && message.Content != null
            && message.Content.GetType() == typeof(ByteArrayContent)
            && expectedResult.SequenceEqual(((ByteArrayContent)message.Content).ReadAsByteArrayAsync().GetAwaiter().GetResult())));
    }

    [Fact]
    public async Task ExecuteSaveReturnsFalseForError() {
        var expectedResult = Encoding.UTF8.GetBytes("This is not actually encrypted data, but normally it would be.");
        
        using var subject = new ApiStorageSite([110, 97, 109, 101], [104, 116, 116, 112, 115, 58, 47, 47, 108, 111, 99, 97, 108, 104, 111, 115, 116, 47], [105, 100], [115, 101, 99, 114, 101, 116]);

        var storageSiteServices = Substitute.For<IStorageSiteServices>();
        storageSiteServices.HttpService.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(new HttpResponseMessage() {
            StatusCode = HttpStatusCode.BadRequest
        });

        var result = await subject.Save(new(expectedResult), storageSiteServices);

        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteSaveInitializesIfNeeded() {
        var expectedResult = Encoding.UTF8.GetBytes("This is not actually encrypted data, but normally it would be.");
        var expectedId = Guid.NewGuid().ToString();

        using var subject = new ApiStorageSite([110, 97, 109, 101], [104, 116, 116, 112, 115, 58, 47, 47, 108, 111, 99, 97, 108, 104, 111, 115, 116, 47]);

        var storageSiteServices = Substitute.For<IStorageSiteServices>();
        storageSiteServices.HttpService.SendAsync(Arg.Is<HttpRequestMessage>(message => message.Method == HttpMethod.Post)).Returns(new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes(expectedId))
        });
        storageSiteServices.HttpService.SendAsync(Arg.Is<HttpRequestMessage>(message => message.Method == HttpMethod.Put)).Returns(new HttpResponseMessage() {
            StatusCode = HttpStatusCode.OK,
            Content = new ByteArrayContent(expectedResult)
        });

        var result = await subject.Save(new(expectedResult), storageSiteServices);

        Assert.True(result);
        Assert.NotEqual(0, subject.Secret.Length);
        Assert.Equal(expectedId, Encoding.UTF8.GetString(subject.DataStoreId));

        var secret = Convert.ToBase64String(subject.Secret);

        await storageSiteServices.HttpService.Received(1).SendAsync(Arg.Is<HttpRequestMessage>(message
            => message.Method == HttpMethod.Post
            && message.RequestUri == new Uri($"https://localhost/")
            && message.Headers.Contains("Secret")
            && message.Headers.GetValues("Secret").SequenceEqual(new string[] { secret })));
        await storageSiteServices.HttpService.Received(1).SendAsync(Arg.Is<HttpRequestMessage>(message
            => message.Method == HttpMethod.Put
            && message.RequestUri == new Uri($"https://localhost/{expectedId}")
            && message.Headers.Contains("Secret")
            && message.Headers.GetValues("Secret").SequenceEqual(new string[] { secret })
            && message.Content != null
            && message.Content.GetType() == typeof(ByteArrayContent)
            && expectedResult.SequenceEqual(((ByteArrayContent)message.Content).ReadAsByteArrayAsync().GetAwaiter().GetResult())));
    }

    [Fact]
    public async Task ExecuteSaveReturnsFalseForInitializationError() {
        var expectedResult = Encoding.UTF8.GetBytes("This is not actually encrypted data, but normally it would be.");

        using var subject = new ApiStorageSite([110, 97, 109, 101], [104, 116, 116, 112, 115, 58, 47, 47, 108, 111, 99, 97, 108, 104, 111, 115, 116, 47]);

        var storageSiteServices = Substitute.For<IStorageSiteServices>();
        storageSiteServices.HttpService.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(new HttpResponseMessage() {
            StatusCode = HttpStatusCode.BadRequest
        });

        var result = await subject.Save(new(expectedResult), storageSiteServices);

        Assert.False(result);

        await storageSiteServices.HttpService.DidNotReceive().SendAsync(Arg.Is<HttpRequestMessage>(message => message.Method == HttpMethod.Put));
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new ApiStorageSite([110, 97, 109, 101], [108, 111, 99, 97, 116, 105, 111, 110], [105, 100], [115, 101, 99, 114, 101, 116]);

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([40, 0, 0, 0, ApiStorageSite.TypeId, 0, 0, 0, 4, 0, 0, 0, 110, 97, 109, 101, 8, 0, 0, 0, 108, 111, 99, 97, 116, 105, 111, 110, 2, 0, 0, 0, 105, 100, 6, 0, 0, 0, 115, 101, 99, 114, 101, 116]), result.GetValue());
    }

    [Fact]
    public void GetLocationThrowsIfDisposed() {
        ApiStorageSite subject;

        using (subject = new([], [])) { }

        Assert.Throws<ObjectDisposedException>(() => { var _ = subject.Location; });
    }

    [Fact]
    public void SetLocationThrowsIfDisposed() {
        ApiStorageSite subject;

        using (subject = new([], [])) { }

        Assert.Throws<ObjectDisposedException>(() => subject.Location = new ReadOnlySpan<byte>([108, 111, 99, 97, 116, 105, 111, 110]));
    }

    [Fact]
    public void GetDataStoreIdThrowsIfDisposed() {
        ApiStorageSite subject;

        using (subject = new([], [])) { }

        Assert.Throws<ObjectDisposedException>(() => { var _ = subject.DataStoreId; });
    }

    [Fact]
    public void GetSecretThrowsIfDisposed() {
        ApiStorageSite subject;

        using (subject = new([], [])) { }

        Assert.Throws<ObjectDisposedException>(() => { var _ = subject.Secret; });
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        ApiStorageSite subject;
        using var plainBytes = new SecureByteList();

        using (subject = new([], [])) { }

        Assert.Throws<ObjectDisposedException>(() => subject.SerializeTo(plainBytes));
    }
}
