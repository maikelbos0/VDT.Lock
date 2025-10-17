using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using VDT.Lock.Api.Data;
using VDT.Lock.Api.Services;

namespace VDT.Lock.Api.Handlers;

public record SaveDataStoreRequest(Guid Id, byte[] Secret, Stream DataStream);

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

        using var dataStream = new MemoryStream();
        await request.DataStream.CopyToAsync(dataStream);
        dataStore.Data = dataStream.ToArray();

        await context.SaveChangesAsync();

        return Results.Ok();
    }
}
