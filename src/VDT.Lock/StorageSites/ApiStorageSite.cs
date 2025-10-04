using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VDT.Lock.StorageSites;

public class ApiStorageSite : StorageSiteBase {
    public const int TypeId = 2;

    public static new ApiStorageSite DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;

        return new(plainSpan.ReadSpan(ref position), plainSpan.ReadSpan(ref position), plainSpan.ReadSpan(ref position), plainSpan.ReadSpan(ref position));
    }

    protected SecureBuffer plainLocationBuffer;
    protected SecureBuffer plainDataStoreIdBuffer;
    protected SecureBuffer plainSecretBuffer;

    public ReadOnlySpan<byte> Location {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return new(plainLocationBuffer.Value);
        }
        set {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            plainLocationBuffer.Dispose();
            plainLocationBuffer = new(value.ToArray());
        }
    }

    public ReadOnlySpan<byte> DataStoreId {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return new(plainDataStoreIdBuffer.Value);
        }
        set {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            plainDataStoreIdBuffer.Dispose();
            plainDataStoreIdBuffer = new(value.ToArray());
        }
    }

    public ReadOnlySpan<byte> Secret {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return new(plainSecretBuffer.Value);
        }
        set {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            plainSecretBuffer.Dispose();
            plainSecretBuffer = new(value.ToArray());
        }
    }

    public override IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [0, plainNameBuffer.Value.Length, plainLocationBuffer.Value.Length, plainDataStoreIdBuffer.Value.Length, plainSecretBuffer.Value.Length];
        }
    }

    public ApiStorageSite(ReadOnlySpan<byte> plainNameSpan, ReadOnlySpan<byte> location) : this(plainNameSpan, location, [], []) { }

    public ApiStorageSite(ReadOnlySpan<byte> plainNameSpan, ReadOnlySpan<byte> location, ReadOnlySpan<byte> dataStoreId, ReadOnlySpan<byte> secret) : base(plainNameSpan) {
        plainLocationBuffer = new(location.ToArray());
        plainDataStoreIdBuffer = new(dataStoreId.ToArray());
        plainSecretBuffer = new(secret.ToArray());
    }

    protected override Task<SecureBuffer?> ExecuteLoad() {
        // If first time: request new storage with secret
        //      -> API generates and returns id
        // Store id + secret

        // Request data using id + passcode
        throw new NotImplementedException();
    }

    protected override Task<bool> ExecuteSave(SecureBuffer encryptedSpan) {
        // If first time: request new storage with secret
        //      -> API generates and returns id
        // Store id + secret

        // Save data using id + secret
        throw new NotImplementedException();
    }

    public override void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(this.GetLength());
        plainBytes.WriteInt(TypeId);
        plainBytes.WriteSecureBuffer(plainNameBuffer);
        plainBytes.WriteSecureBuffer(plainLocationBuffer);
        plainBytes.WriteSecureBuffer(plainDataStoreIdBuffer);
        plainBytes.WriteSecureBuffer(plainSecretBuffer);
    }
}
