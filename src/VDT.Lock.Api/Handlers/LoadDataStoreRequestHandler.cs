using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using VDT.Lock.Api.Data;
using VDT.Lock.Api.Services;

namespace VDT.Lock.Api.Handlers;

public record LoadDataStoreRequest(Guid Id, byte[] Secret);

public class LoadDataStoreRequestHandler {
    private readonly LockContext context;
    private readonly ISecretHasher secretHasher;

    public LoadDataStoreRequestHandler(LockContext context, ISecretHasher secretHasher) {
        this.context = context;
        this.secretHasher = secretHasher;
    }

    public async Task<IResult> Handle(LoadDataStoreRequest request, CancellationToken cancellationToken) {
        var dataStore = await context.DataStores
            .SingleOrDefaultAsync(dataStore => dataStore.Id == request.Id, cancellationToken);

        if (dataStore == null || !secretHasher.VerifySecret(dataStore.SecretSalt, request.Secret, dataStore.SecretHash)) {
            return Results.Unauthorized();
        }

        return Results.Ok(dataStore.Data);
    }
}
