namespace VDT.Lock.Api.Configuration;

public class AppSettings {
    public required string ConnectionString { get; set; }

    public required int MaxRequestBodySize { get; set; }
}
