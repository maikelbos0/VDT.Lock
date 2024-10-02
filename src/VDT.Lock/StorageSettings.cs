using System.Collections.Concurrent;
using System.Text;

namespace VDT.Lock;

public sealed class StorageSettings : IDisposable {
    public static StorageSettings DeserializeFrom(ReadOnlySpan<byte> plainSettingsSpan) {
        var storageSettings = new StorageSettings();
        var position = 0;

        while (position < plainSettingsSpan.Length) {
            storageSettings.plainSettingsBuffers[Encoding.UTF8.GetString(plainSettingsSpan.ReadSpan(ref position))] = plainSettingsSpan.ReadSecureBuffer(ref position);
        }

        return storageSettings;
    }

    private readonly ConcurrentDictionary<string, SecureBuffer> plainSettingsBuffers = [];

    public StorageSettings() { }

    public ReadOnlySpan<byte> Get(string key)
        => new(plainSettingsBuffers[key].Value);

    public void Set(string key, ReadOnlySpan<byte> valueSpan) {
        var plainNewValueBuffer = new SecureBuffer(valueSpan.ToArray());

        plainSettingsBuffers.AddOrUpdate(key, plainNewValueBuffer, (key, plainPreviousValueBuffer) => {
            plainPreviousValueBuffer.Dispose();
            return plainNewValueBuffer;
        });
    }

    public void SerializeTo(SecureByteList plainSettingsBytes) {
        var settingsSnapshot = plainSettingsBuffers
            .ToArray()
            .OrderBy(pair => pair.Key)
            .Select(pair => new {
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
        foreach (var value in plainSettingsBuffers.Values) {
            value.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
