using System.Collections.Concurrent;

namespace VDT.Lock;

public sealed class StorageSettings : IDisposable {
    private readonly ConcurrentDictionary<string, SecureBuffer> settings = [];

    public StorageSettings(ReadOnlySpan<byte> settingsSpan) {
        var position = 0;

        while (position < settingsSpan.Length) {
            settings[settingsSpan.ReadString(ref position)] = settingsSpan.ReadSecureBuffer(ref position);
        }
    }

    public ReadOnlySpan<byte> GetSetting(string key)
        => new ReadOnlySpan<byte>(settings[key].Value);

    public void SetSetting(string key, ReadOnlySpan<byte> valueSpan) {
        var value = new SecureBuffer(valueSpan.ToArray());

        settings.AddOrUpdate(key, value, (key, previousValue) => {
            previousValue.Dispose();
            return value;
        });
    }

    public void Dispose() {
        foreach (var value in settings.Values) {
            value.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
