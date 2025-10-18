using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using VDT.Lock.Api.Configuration;
using VDT.Lock.Api.Data;
using VDT.Lock.Api.Services;

namespace VDT.Lock.Api.Handlers;

public record SaveDataStoreRequest(Guid Id, byte[] Secret, Stream DataStream, long? DataLength);

public class SaveDataStoreRequestHandler {
    private readonly LockContext context;
    private readonly ISecretHasher secretHasher;
    private readonly IOptionsSnapshot<AppSettings> appSettings;

    public SaveDataStoreRequestHandler(LockContext context, ISecretHasher secretHasher, IOptionsSnapshot<AppSettings> appSettings) {
        this.context = context;
        this.secretHasher = secretHasher;
        this.appSettings = appSettings;
    }

    public async Task<IResult> Handle(SaveDataStoreRequest request) {
        if (request.DataLength == null || request.DataLength > appSettings.Value.MaxRequestBodySize) {
            return Results.BadRequest();
        }

        var dataStore = await context.DataStores
            .AsTracking()
            .SingleOrDefaultAsync(dataStore => dataStore.Id == request.Id);

        if (dataStore == null || !secretHasher.VerifySecret(dataStore.SecretSalt, request.Secret, dataStore.SecretHash)) {
            return Results.Unauthorized();
        }

        dataStore.Data = await ReadData(request.DataStream, (int)request.DataLength.Value);

        await context.SaveChangesAsync();

        return Results.Ok();
    }

    private static async Task<byte[]> ReadData(Stream dataStream, int dataLength) {
        const int bufferSize = 65536;

        var data = new byte[dataLength];
        var position = 0;
        var count = 0;

        do {
            position += count;
            count = Math.Min(bufferSize, dataLength - position);
        }
        while ((count = await dataStream.ReadAsync(data.AsMemory(position, count))) != 0);

        return data;
    }
}
