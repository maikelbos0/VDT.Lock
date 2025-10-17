using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.IO;
using System.Threading.Tasks;
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

        var subject = new SaveDataStoreRequestHandler(context, secretHasher);
        var request = new SaveDataStoreRequest(id, [], dataStream);

        var result = await subject.Handle(request);

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task ReturnsUnauthorizedForInvalidId() {
        using var dataStream = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7]);

        var context = LockContextProvider.Provide();

        var secretHasher = Substitute.For<ISecretHasher>();
        secretHasher.VerifySecret(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>()).Returns(true);

        var subject = new SaveDataStoreRequestHandler(context, secretHasher);
        var request = new SaveDataStoreRequest(Guid.NewGuid(), [], dataStream);

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

        var subject = new SaveDataStoreRequestHandler(context, secretHasher);
        var request = new SaveDataStoreRequest(id, [], dataStream);

        var result = await subject.Handle(request);

        Assert.IsType<UnauthorizedHttpResult>(result);
    }
}
