using System.Collections.Concurrent;
using System.Text;

namespace VDT.Lock;

public sealed class StorageSettings : IDisposable {
    private readonly ConcurrentDictionary<string, SecureBuffer> plainSettingsBuffer = [];

    public StorageSettings() { }

    public StorageSettings(ReadOnlySpan<byte> plainSettingsSpan) {
        var position = 0;

        while (position < plainSettingsSpan.Length) {
            plainSettingsBuffer[Encoding.UTF8.GetString(plainSettingsSpan.ReadSpan(ref position))] = plainSettingsSpan.ReadSecureBuffer(ref position);
        }
    }

    public ReadOnlySpan<byte> Get(string key)
        => new(plainSettingsBuffer[key].Value);

    public void Set(string key, ReadOnlySpan<byte> valueSpan) {
        var plainNewValueBuffer = new SecureBuffer(valueSpan.ToArray());

        plainSettingsBuffer.AddOrUpdate(key, plainNewValueBuffer, (key, plainPreviousValueBuffer) => {
            plainPreviousValueBuffer.Dispose();
            return plainNewValueBuffer;
        });
    }

    public void SerializeTo(SecureByteList plainSettingsBytes) {
        var settingsSnapshot = plainSettingsBuffer.ToArray().Select(pair => new {
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
        foreach (var value in plainSettingsBuffer.Values) {
            value.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
