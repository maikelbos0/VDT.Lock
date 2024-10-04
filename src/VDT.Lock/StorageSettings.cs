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

    private readonly Dictionary<string, SecureBuffer> plainSettingsBuffers = [];

    public StorageSettings() { }

    public ReadOnlySpan<byte> Get(string key)
        => new(plainSettingsBuffers[key].Value);

    public void Set(string key, ReadOnlySpan<byte> valueSpan) {
        if (plainSettingsBuffers.TryGetValue(key, out var plainOldValueBuffer)) {
            plainOldValueBuffer.Dispose();
        }

        plainSettingsBuffers[key] = new SecureBuffer(valueSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainSettingsBytes) {
        var serializableSettings = plainSettingsBuffers
            .OrderBy(pair => pair.Key)
            .Select(pair => new {
                Name = Encoding.UTF8.GetBytes(pair.Key),
                pair.Value
            });

        plainSettingsBytes.WriteInt(serializableSettings.Sum(setting => setting.Name.Length + setting.Value.Value.Length + 8));

        foreach (var pair in serializableSettings) {
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
