using System;
using System.Collections.Generic;
using System.Linq;

namespace VDT.Lock;

public sealed class DataIdentity : IData<DataIdentity>, IEquatable<DataIdentity>, IDisposable {
    public static bool operator ==(DataIdentity? a, DataIdentity? b) {
        if (a is null) {
            return b is null;
        }

        return a.Equals(b);
    }

    public static bool operator !=(DataIdentity? a, DataIdentity? b) => !(a == b);

    public static T SelectNewest<T>(IEnumerable<T> candidates) where T : IIdentifiableData<T> {
        T result = default!;
        var newestVersion = long.MinValue;

        foreach (var candidate in candidates) {
            var version = candidate.Identity.Version[0]
                | ((long)candidate.Identity.Version[1] << 8)
                | ((long)candidate.Identity.Version[2] << 16)
                | ((long)candidate.Identity.Version[3] << 24)
                | ((long)candidate.Identity.Version[4] << 32)
                | ((long)candidate.Identity.Version[5] << 40)
                | ((long)candidate.Identity.Version[6] << 48)
                | ((long)candidate.Identity.Version[7] << 56);

            if (version > newestVersion) {
                result = candidate;
                newestVersion = version;
            }
        }

        return result;
    }

    public static DataIdentity DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;

        return new(plainSpan.ReadSpan(ref position), plainSpan.ReadSpan(ref position));
    }

    private readonly SecureBuffer plainKeyBuffer;
    private SecureBuffer plainVersionBuffer;

    public bool IsDisposed { get; private set; }

    public ReadOnlySpan<byte> Key {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return new(plainKeyBuffer.Value);
        }
    }

    public ReadOnlySpan<byte> Version {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return new(plainVersionBuffer.Value);
        }
    }

    public IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [plainKeyBuffer.Value.Length, plainVersionBuffer.Value.Length];
        }
    }

    public DataIdentity() {
        plainKeyBuffer = new(Guid.NewGuid().ToByteArray());
        plainVersionBuffer = new(DateTimeOffset.UtcNow.ToVersion());
    }

    private DataIdentity(ReadOnlySpan<byte> plainKeySpan, ReadOnlySpan<byte> plainVersionSpan) {
        plainKeyBuffer = new(plainKeySpan.ToArray());
        plainVersionBuffer = new(plainVersionSpan.ToArray());
    }

    public void Update() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainVersionBuffer.Dispose();
        plainVersionBuffer = new(DateTimeOffset.UtcNow.ToVersion());
    }

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(this.GetLength());
        plainBytes.WriteSecureBuffer(plainKeyBuffer);
        plainBytes.WriteSecureBuffer(plainVersionBuffer);
    }

    public override int GetHashCode() {
        unchecked {
            var result = 0;

            foreach (byte b in plainKeyBuffer.Value) {
                result = (result * 31) ^ b;
            }

            return result;
        }
    }

    public override bool Equals(object? obj) => Equals(obj as DataIdentity);

    public bool Equals(DataIdentity? other) {
        if (other is null) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        return plainKeyBuffer.Value.SequenceEqual(other.plainKeyBuffer.Value);
    }

    public void Dispose() {
        plainKeyBuffer.Dispose();
        plainVersionBuffer.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
