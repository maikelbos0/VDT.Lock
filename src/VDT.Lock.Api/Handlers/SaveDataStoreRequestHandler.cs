using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using VDT.Lock.Api.Data;
using VDT.Lock.Api.Services;

namespace VDT.Lock.Api.Handlers;

public record SaveDataStoreRequest(Guid Id, byte[] Secret, byte[] Data);

public class SaveDataStoreRequestHandler {
    private readonly LockContext context;
    private readonly ISecretHasher secretHasher;

    public SaveDataStoreRequestHandler(LockContext context, ISecretHasher secretHasher) {
        this.context = context;
        this.secretHasher = secretHasher;
    }

    public async Task<IResult> Handle(SaveDataStoreRequest request) {
        var dataStore = await context.DataStores
            .AsTracking()
            .SingleOrDefaultAsync(dataStore => dataStore.Id == request.Id);

        if (dataStore == null || !secretHasher.VerifySecret(dataStore.SecretSalt, request.Secret, dataStore.SecretHash)) {
            return Results.Unauthorized();
        }

        dataStore.Data = request.Data;
        await context.SaveChangesAsync();

        return Results.Ok();
    }
}
