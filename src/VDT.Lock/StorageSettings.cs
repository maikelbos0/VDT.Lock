using System;
using System.Collections.Generic;
using System.Linq;
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

    public bool IsDisposed { get; private set; }

    public int Length {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return plainSettingsBuffers.Sum(pair => Encoding.UTF8.GetByteCount(pair.Key) + pair.Value.Value.Length + 8);
        }
    }

    public StorageSettings() { }

    public ReadOnlySpan<byte> Get(string key) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return new(plainSettingsBuffers[key].Value);
    }

    public void Set(string key, ReadOnlySpan<byte> valueSpan) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (plainSettingsBuffers.TryGetValue(key, out var plainOldValueBuffer)) {
            plainOldValueBuffer.Dispose();
        }

        plainSettingsBuffers[key] = new SecureBuffer(valueSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainSettingsBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainSettingsBytes.WriteInt(Length);

        foreach (var pair in plainSettingsBuffers.OrderBy(pair => pair.Key)) {
            plainSettingsBytes.WriteSpan(Encoding.UTF8.GetBytes(pair.Key));
            plainSettingsBytes.WriteSecureBuffer(pair.Value);
        }
    }

    public void Dispose() {
        foreach (var value in plainSettingsBuffers.Values) {
            value.Dispose();
        }
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
