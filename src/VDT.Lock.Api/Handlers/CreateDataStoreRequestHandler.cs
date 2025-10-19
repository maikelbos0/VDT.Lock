using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDT.Lock.Api.Data;
using VDT.Lock.Api.Services;

namespace VDT.Lock.Api.Handlers;

public record CreateDataStoreRequest(byte[] Secret);

public class CreateDataStoreRequestHandler {
    private readonly LockContext context;
    private readonly ISecretHasher secretHasher;

    public CreateDataStoreRequestHandler(LockContext context, ISecretHasher secretHasher) {
        this.context = context;
        this.secretHasher = secretHasher;
    }

    public async Task<IResult> Handle(CreateDataStoreRequest request, CancellationToken cancellationToken) {
        var id = Guid.NewGuid();
        var (secretSalt, secretHash) = secretHasher.HashSecret(request.Secret);

        context.DataStores.Add(new() {
            Id = id,
            SecretSalt = secretSalt,
            SecretHash = secretHash
        });

        await context.SaveChangesAsync(cancellationToken);

        return new RawOkResult(Encoding.UTF8.GetBytes(id.ToString()));
    }
}
