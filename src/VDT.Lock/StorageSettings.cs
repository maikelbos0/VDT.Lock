using System.Collections.Concurrent;
using System.Text;

namespace VDT.Lock;

public sealed class StorageSettings : IDisposable {
    private readonly ConcurrentDictionary<string, SecureBuffer> settings = [];

    public StorageSettings() { }

    public StorageSettings(ReadOnlySpan<byte> plainSettingsSpan) {
        var position = 0;

        while (position < plainSettingsSpan.Length) {
            settings[Encoding.UTF8.GetString(plainSettingsSpan.ReadSpan(ref position))] = plainSettingsSpan.ReadSecureBuffer(ref position);
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

    public void SerializeTo(SecureByteList plainSettingsBytes) {
        var settingsSnapshot = settings.ToArray().Select(pair => new {
            Name = Encoding.UTF8.GetBytes(pair.Key),
            pair.Value
        });

        plainSettingsBytes.WriteInt(settingsSnapshot.Sum(setting => setting.Name.Length + setting.Value.Value.Length + 8));

        foreach (var pair in settingsSnapshot) {
            plainSettingsBytes.WriteSpan(pair.Name);
            plainSettingsBytes.WriteSecureBuffer(pair.Value);
        }
    }

    public void Dispose() {
        foreach (var value in settings.Values) {
            value.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
