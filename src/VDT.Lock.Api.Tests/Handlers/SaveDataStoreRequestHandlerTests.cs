using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VDT.Lock.Api.Configuration;
using VDT.Lock.Api.Handlers;
using VDT.Lock.Api.Services;
using VDT.Lock.Api.Tests.Data;
using Xunit;

namespace VDT.Lock.Api.Tests.Handlers;

public class SaveDataStoreRequestHandlerTests {
    [Fact]
    public async Task SavesData() {
        using var dataStream = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7]);
        var id = Guid.NewGuid();

        var context = LockContextProvider.Provide();
        context.DataStores.Add(new() {
            Id = id,
            SecretSalt = [],
            SecretHash = []
        });
        await context.SaveChangesAsync();

        var secretHasher = Substitute.For<ISecretHasher>();
        secretHasher.VerifySecret(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>()).Returns(true);

        var appSettings = Substitute.For<IOptionsSnapshot<AppSettings>>();
        appSettings.Value.Returns(new AppSettings() {
            ConnectionString = "test",
            MaxRequestBodySize = 8
        });

        var subject = new SaveDataStoreRequestHandler(context, secretHasher, appSettings);
        var request = new SaveDataStoreRequest(id, [], dataStream, 8);

        var result = await subject.Handle(request);

        Assert.Equal([0, 1, 2, 3, 4, 5, 6, 7], context.DataStores.Single().Data);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task SucceedsForLargerDataLengthThanDataStreamLength() {
        using var dataStream = new MemoryStream([0, 1, 2, 3, 4, 5, 6]);
        var id = Guid.NewGuid();

        var context = LockContextProvider.Provide();
        context.DataStores.Add(new() {
            Id = id,
            SecretSalt = [],
            SecretHash = []
        });
        await context.SaveChangesAsync();

        var secretHasher = Substitute.For<ISecretHasher>();
        secretHasher.VerifySecret(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>()).Returns(true);

        var appSettings = Substitute.For<IOptionsSnapshot<AppSettings>>();
        appSettings.Value.Returns(new AppSettings() {
            ConnectionString = "test",
            MaxRequestBodySize = 8
        });

        var subject = new SaveDataStoreRequestHandler(context, secretHasher, appSettings);
        var request = new SaveDataStoreRequest(id, [], dataStream, 8);

        var result = await subject.Handle(request);

        Assert.Equal([0, 1, 2, 3, 4, 5, 6, 0], context.DataStores.Single().Data);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task SucceedsForSmallerDataLengthThanDataStreamLength() {
        using var dataStream = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7]);
        var id = Guid.NewGuid();

        var context = LockContextProvider.Provide();
        context.DataStores.Add(new() {
            Id = id,
            SecretSalt = [],
            SecretHash = []
        });
        await context.SaveChangesAsync();

        var secretHasher = Substitute.For<ISecretHasher>();
        secretHasher.VerifySecret(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>()).Returns(true);

        var appSettings = Substitute.For<IOptionsSnapshot<AppSettings>>();
        appSettings.Value.Returns(new AppSettings() {
            ConnectionString = "test",
            MaxRequestBodySize = 8
        });

        var subject = new SaveDataStoreRequestHandler(context, secretHasher, appSettings);
        var request = new SaveDataStoreRequest(id, [], dataStream, 7);

        var result = await subject.Handle(request);

        Assert.Equal([0, 1, 2, 3, 4, 5, 6], context.DataStores.Single().Data);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task UsesReadCountWhenReadingDataStream() {
        var dataStream = Substitute.For<Stream>();
        dataStream.ReadAsync(Arg.Any<Memory<byte>>(), default).Returns(4, 4, 0);
        
        var id = Guid.NewGuid();

        var context = LockContextProvider.Provide();
        context.DataStores.Add(new() {
            Id = id,
            SecretSalt = [],
            SecretHash = []
        });
        await context.SaveChangesAsync();

        var secretHasher = Substitute.For<ISecretHasher>();
        secretHasher.VerifySecret(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>()).Returns(true);

        var appSettings = Substitute.For<IOptionsSnapshot<AppSettings>>();
        appSettings.Value.Returns(new AppSettings() {
            ConnectionString = "test",
            MaxRequestBodySize = 8
        });

        var subject = new SaveDataStoreRequestHandler(context, secretHasher, appSettings);
        var request = new SaveDataStoreRequest(id, [], dataStream, 8);

        var result = await subject.Handle(request);

        _ = await dataStream.Received(1).ReadAsync(Arg.Is<Memory<byte>>(memory => memory.Length == 8));
        _ = await dataStream.Received(1).ReadAsync(Arg.Is<Memory<byte>>(memory => memory.Length == 4));
        _ = await dataStream.Received(1).ReadAsync(Arg.Is<Memory<byte>>(memory => memory.Length == 0));
        
        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task ReturnsBadRequestForNullDataLength() {
        using var dataStream = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7]);
        var id = Guid.NewGuid();

        var context = LockContextProvider.Provide();

        var secretHasher = Substitute.For<ISecretHasher>();
        secretHasher.VerifySecret(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>()).Returns(true);

        var appSettings = Substitute.For<IOptionsSnapshot<AppSettings>>();
        appSettings.Value.Returns(new AppSettings() {
            ConnectionString = "test",
            MaxRequestBodySize = 8
        });

        var subject = new SaveDataStoreRequestHandler(context, secretHasher, appSettings);
        var request = new SaveDataStoreRequest(id, [], dataStream, null);

        var result = await subject.Handle(request);

        Assert.IsType<BadRequest>(result);
    }

    [Fact]
    public async Task ReturnsBadRequestForTooLargeDataLength() {
        using var dataStream = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7]);
        var id = Guid.NewGuid();

        var context = LockContextProvider.Provide();

        var secretHasher = Substitute.For<ISecretHasher>();
        secretHasher.VerifySecret(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>()).Returns(true);

        var appSettings = Substitute.For<IOptionsSnapshot<AppSettings>>();
        appSettings.Value.Returns(new AppSettings() {
            ConnectionString = "test",
            MaxRequestBodySize = 8
        });

        var subject = new SaveDataStoreRequestHandler(context, secretHasher, appSettings);
        var request = new SaveDataStoreRequest(id, [], dataStream, 9);

        var result = await subject.Handle(request);

        Assert.IsType<BadRequest>(result);
    }

    [Fact]
    public async Task ReturnsUnauthorizedForInvalidId() {
        using var dataStream = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7]);

        var context = LockContextProvider.Provide();

        var secretHasher = Substitute.For<ISecretHasher>();
        secretHasher.VerifySecret(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>()).Returns(true);

        var appSettings = Substitute.For<IOptionsSnapshot<AppSettings>>();
        appSettings.Value.Returns(new AppSettings() {
            ConnectionString = "test",
            MaxRequestBodySize = 8
        });

        var subject = new SaveDataStoreRequestHandler(context, secretHasher, appSettings);
        var request = new SaveDataStoreRequest(Guid.NewGuid(), [], dataStream, 8);

        var result = await subject.Handle(request);

        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task ReturnsUnauthorizedForInvalidSecret() {
        using var dataStream = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7]);
        var id = Guid.NewGuid();

        var context = LockContextProvider.Provide();
        context.DataStores.Add(new() {
            Id = id,
            SecretSalt = [],
            SecretHash = []
        });
        await context.SaveChangesAsync();

        var secretHasher = Substitute.For<ISecretHasher>();
        secretHasher.VerifySecret(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>()).Returns(false);

        var appSettings = Substitute.For<IOptionsSnapshot<AppSettings>>();
        appSettings.Value.Returns(new AppSettings() {
            ConnectionString = "test",
            MaxRequestBodySize = 8
        });

        var subject = new SaveDataStoreRequestHandler(context, secretHasher, appSettings);
        var request = new SaveDataStoreRequest(id, [], dataStream, 8);

        var result = await subject.Handle(request);

        Assert.IsType<UnauthorizedHttpResult>(result);
    }
}
