using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VDT.Lock;

public abstract class StorageSiteBase : BaseData, IDisposable {
    protected readonly StorageSettings storageSettings;

    public bool IsDisposed { get; private set; }

    public override IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [Encoding.UTF8.GetByteCount(GetType().Name), storageSettings.Length];
        }
    }

    public StorageSiteBase(StorageSettings storageSettings) {
        this.storageSettings = storageSettings;
    }

    public Task<SecureBuffer> Load() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return ExecuteLoad();
    }

    protected abstract Task<SecureBuffer> ExecuteLoad();

    public Task Save(ReadOnlySpan<byte> encryptedData) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return ExecuteSave(encryptedData);
    }

    protected abstract Task ExecuteSave(ReadOnlySpan<byte> encryptedData);

    public override void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(Length);
        plainBytes.WriteSpan(Encoding.UTF8.GetBytes(GetType().Name));
        storageSettings.SerializeTo(plainBytes);
    }

    public void Dispose() {
        storageSettings.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
