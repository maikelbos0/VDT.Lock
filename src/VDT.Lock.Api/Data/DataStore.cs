using System;

namespace VDT.Lock.Api.Data;

public class DataStore {
    public required Guid Id { get; set; }

    public string? Password { get; set; }

    public byte[]? Data { get; set; }
}
