using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VDT.Lock.Services;

namespace VDT.Lock.StorageSites;

public class ApiStorageSite : StorageSiteBase {
    public const int TypeId = 2;
    public const int SecretSizeInBytes = 32;

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
    }

    public ReadOnlySpan<byte> Secret {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return new(plainSecretBuffer.Value);
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

    protected override async Task<SecureBuffer?> ExecuteLoad(IStorageSiteServices storageSiteServices) {
        if (!(await Initialize(storageSiteServices))) {
            return null;
        }

        var request = new HttpRequestMessage() {
            RequestUri = new(Encoding.UTF8.GetString(plainLocationBuffer.Value).TrimEnd('/') + "/" + Encoding.UTF8.GetString(plainDataStoreIdBuffer.Value)),
            Method = HttpMethod.Get,
            Headers = {
                {  "Secret", Convert.ToBase64String(plainSecretBuffer.Value) }
            }
        };
        var response = await storageSiteServices.HttpService.SendAsync(request);

        if (response.IsSuccessStatusCode) {
            return new(await response.Content.ReadAsByteArrayAsync());
        }

        return null;
    }

    protected override async Task<bool> ExecuteSave(SecureBuffer encryptedSpan, IStorageSiteServices storageSiteServices) {
        if (!(await Initialize(storageSiteServices))) {
            return false;
        }

        var request = new HttpRequestMessage() {
            RequestUri = new(Encoding.UTF8.GetString(plainLocationBuffer.Value).TrimEnd('/') + "/" + Encoding.UTF8.GetString(plainDataStoreIdBuffer.Value)),
            Method = HttpMethod.Put,
            Headers = {
                {  "Secret", Convert.ToBase64String(plainSecretBuffer.Value) }
            },
            Content = new ByteArrayContent(encryptedSpan.Value)
        };
        var response = await storageSiteServices.HttpService.SendAsync(request);

        return response.IsSuccessStatusCode;
    }

    private async Task<bool> Initialize(IStorageSiteServices storageSiteServices) {
        if (plainDataStoreIdBuffer.Value.Length > 0) {
            return true;
        }

        if (plainSecretBuffer.Value.Length == 0) {
            plainSecretBuffer.Dispose();
            plainSecretBuffer = new(RandomNumberGenerator.GetBytes(SecretSizeInBytes));
        }

        var request = new HttpRequestMessage() {
            RequestUri = new(Encoding.UTF8.GetString(plainLocationBuffer.Value)),
            Method = HttpMethod.Post,
            Headers = {
                {  "Secret", Convert.ToBase64String(plainSecretBuffer.Value) }
            }
        };
        var response = await storageSiteServices.HttpService.SendAsync(request);

        if (response.IsSuccessStatusCode) {
            plainDataStoreIdBuffer.Dispose();
            plainDataStoreIdBuffer = new(await response.Content.ReadAsByteArrayAsync());

            return true;
        }

        return false;
    }

    public override void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(this.Length);
        plainBytes.WriteInt(TypeId);
        plainBytes.WriteSecureBuffer(plainNameBuffer);
        plainBytes.WriteSecureBuffer(plainLocationBuffer);
        plainBytes.WriteSecureBuffer(plainDataStoreIdBuffer);
        plainBytes.WriteSecureBuffer(plainSecretBuffer);
    }

    public override void Dispose() {
        base.Dispose();
        plainLocationBuffer.Dispose();
        plainDataStoreIdBuffer.Dispose();
        plainSecretBuffer.Dispose();
        GC.SuppressFinalize(this);
    }
}
