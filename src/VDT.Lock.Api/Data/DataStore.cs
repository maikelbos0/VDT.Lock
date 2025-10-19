using System;

namespace VDT.Lock.Api.Data;

public class DataStore {
    public required Guid Id { get; set; }

    public required byte[] SecretSalt { get; set; }

    public required byte[] SecretHash { get; set; }

    public byte[]? Data { get; set; }
}
