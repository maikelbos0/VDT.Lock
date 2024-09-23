namespace VDT.Lock;

public sealed class StorageSettings : IDisposable {
    private Dictionary<string, SecureBuffer> settings = [];

    public StorageSettings(ReadOnlySpan<byte> settingsSpan) {
        var position = 0;

        while (position < settingsSpan.Length) {
            settings.Add(settingsSpan.ReadString(ref position), settingsSpan.ReadSecureBuffer(ref position));
        }
    }

    public ReadOnlySpan<byte> GetSetting(string key)
        => new ReadOnlySpan<byte>(settings[key].Value);

    public void Dispose() {
        foreach (var value in settings.Values) {
            value.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
