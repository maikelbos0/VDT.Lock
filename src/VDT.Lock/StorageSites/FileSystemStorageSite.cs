using System;
using System.Threading.Tasks;
using System.Collections.Generic;

#if !BROWSER
using System.IO;
using System.Text;
#endif

namespace VDT.Lock.StorageSites;

public class FileSystemStorageSite : StorageSiteBase {
    public const int TypeId = 1;

    public static FileSystemStorageSite DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;

        return new(plainSpan.ReadSpan(ref position), plainSpan.ReadSpan(ref position));
    }

    protected SecureBuffer plainLocationBuffer;

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

    public override IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [0, plainNameBuffer.Value.Length, plainLocationBuffer.Value.Length];
        }
    }

    public FileSystemStorageSite(ReadOnlySpan<byte> plainNameSpan, ReadOnlySpan<byte> location) : base(plainNameSpan) {
        plainLocationBuffer = new(location.ToArray());
    }

#if BROWSER
    protected override Task<SecureBuffer?> ExecuteLoad()
        => Task.FromResult<SecureBuffer?>(null);
#else
    protected override Task<SecureBuffer?> ExecuteLoad() {
        using var fileStream = File.OpenRead(Encoding.UTF8.GetString(Location));
        using var encryptedBytes = new SecureByteList(fileStream);

        return Task.FromResult<SecureBuffer?>(encryptedBytes.ToBuffer());
    }
#endif

#if BROWSER
    protected override Task<bool> ExecuteSave(SecureBuffer encryptedBuffer)
        => Task.FromResult(false);
#else
    protected override Task<bool> ExecuteSave(SecureBuffer encryptedBuffer) {
        using var fileStream = File.Create(Encoding.UTF8.GetString(Location));

        fileStream.Write(encryptedBuffer.Value);

        return Task.FromResult(true);
    }
#endif

    public override void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(this.GetLength());
        plainBytes.WriteInt(TypeId);
        plainBytes.WriteSecureBuffer(plainNameBuffer);
        plainBytes.WriteSecureBuffer(plainLocationBuffer);
    }

    public override void Dispose() {
        base.Dispose();
        plainLocationBuffer.Dispose();
        GC.SuppressFinalize(this);
    }
}
