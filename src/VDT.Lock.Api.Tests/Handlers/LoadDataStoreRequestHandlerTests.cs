using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Threading;
using System.Threading.Tasks;
using VDT.Lock.Api.Handlers;
using VDT.Lock.Api.Services;
using VDT.Lock.Api.Tests.Data;
using Xunit;

namespace VDT.Lock.Api.Tests.Handlers;

public class LoadDataStoreRequestHandlerTests {
    [Fact]
    public async Task ReturnsData() {
        var id = Guid.NewGuid();
        var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };

        var context = LockContextProvider.Provide();
        context.DataStores.Add(new() {
            Id = id,
            SecretSalt = [],
            SecretHash = [],
            Data = data
        });
        await context.SaveChangesAsync();

        var secretHasher = Substitute.For<ISecretHasher>();
        secretHasher.VerifySecret(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>()).Returns(true);

        var subject = new LoadDataStoreRequestHandler(context, secretHasher);
        var request = new LoadDataStoreRequest(id, []);

        var result = await subject.Handle(request, CancellationToken.None);

        Assert.Equal(data, Assert.IsType<byte[]>(Assert.IsType<OkObjectResult>(result).Value));
    }

    [Fact]
    public async Task ReturnsNullIfNoDataAvailable() {
        var id = Guid.NewGuid();

        var context = LockContextProvider.Provide();
        context.DataStores.Add(new() {
            Id = id,
            SecretSalt = [],
            SecretHash = [],
            Data = null
        });
        await context.SaveChangesAsync();

        var secretHasher = Substitute.For<ISecretHasher>();
        secretHasher.VerifySecret(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>()).Returns(true);

        var subject = new LoadDataStoreRequestHandler(context, secretHasher);
        var request = new LoadDataStoreRequest(id, []);

        var result = await subject.Handle(request, CancellationToken.None);

        Assert.Null(Assert.IsType<OkObjectResult>(result).Value);
    }

    [Fact]
    public async Task ReturnsUnauthorizedForInvalidId() {
        var context = LockContextProvider.Provide();

        var secretHasher = Substitute.For<ISecretHasher>();
        secretHasher.VerifySecret(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>()).Returns(true);

        var subject = new LoadDataStoreRequestHandler(context, secretHasher);
        var request = new LoadDataStoreRequest(Guid.NewGuid(), []);

        var result = await subject.Handle(request, CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task ReturnsUnauthorizedForInvalidSecret() {
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

        var subject = new LoadDataStoreRequestHandler(context, secretHasher);
        var request = new LoadDataStoreRequest(id, []);

        var result = await subject.Handle(request, CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }
}
