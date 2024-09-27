using System.Collections.Concurrent;

namespace VDT.Lock;

public sealed class StorageSettings : IDisposable {
    private readonly ConcurrentDictionary<string, SecureBuffer> settings = [];

    public StorageSettings() { }

    public StorageSettings(ReadOnlySpan<byte> plainSettingsSpan) {
        var position = 0;

        while (position < plainSettingsSpan.Length) {
            settings[SettingsSerializer.ReadString(plainSettingsSpan, ref position)] = SettingsSerializer.ReadSecureBuffer(plainSettingsSpan, ref position);
        }
    }

    public ReadOnlySpan<byte> Get(string key)
        => new(settings[key].Value);

    public void Set(string key, ReadOnlySpan<byte> valueSpan) {
        var value = new SecureBuffer(valueSpan.ToArray());

        settings.AddOrUpdate(key, value, (key, previousValue) => {
            previousValue.Dispose();
            return value;
        });
    }

    public SecureBuffer Serialize() {
        var plainBytes = new SecureByteList();
        var settingsSnapshot = settings.ToArray();

        foreach (var pair in settingsSnapshot) {
            SettingsSerializer.WriteString(plainBytes, pair.Key);
            SettingsSerializer.WriteSecureBuffer(plainBytes, pair.Value);
        }

        return plainBytes.ToBuffer();
    }

    public void Dispose() {
        foreach (var value in settings.Values) {
            value.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
