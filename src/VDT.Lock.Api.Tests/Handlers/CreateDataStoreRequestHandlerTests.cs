using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using VDT.Lock.Api.Handlers;
using VDT.Lock.Api.Services;
using VDT.Lock.Api.Tests.Data;
using Xunit;

namespace VDT.Lock.Api.Tests.Handlers;

public class CreateDataStoreRequestHandlerTests {
    [Fact]
    public async Task CreatesDataStore() {
        var secret = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        var secretSalt = new byte[] { 0, 1, 2, 3 };
        var secretHash = new byte[] { 4, 5, 6, 7 };
        var context = LockContextProvider.Provide();
        var secretHasher = Substitute.For<ISecretHasher>();

        secretHasher.HashSecret(secret).Returns((secretSalt, secretHash));

        var subject = new CreateDataStoreRequestHandler(context, secretHasher);
        var request = new CreateDataStoreRequest(secret);

        var result = await subject.Handle(request, CancellationToken.None);

        var dataStore = Assert.Single(context.DataStores);
        Assert.Equal(Assert.IsType<RawOkResult>(result).Value, dataStore.Id.ToByteArray());
        Assert.Equal(secretSalt, dataStore.SecretSalt);
        Assert.Equal(secretHash, dataStore.SecretHash);
        Assert.Null(dataStore.Data);
    }
}
