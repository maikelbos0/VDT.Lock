using System;
using System.Text;
using System.Threading.Tasks;

namespace VDT.Lock;

public abstract class StorageSiteBase : IDisposable {
    protected readonly StorageSettings storageSettings;

    public bool IsDisposed { get; private set; }

    public int Length {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return Encoding.UTF8.GetByteCount(GetType().Name)
                + storageSettings.Length
                + 8;
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

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteSpan(Encoding.UTF8.GetBytes(GetType().Name));
        storageSettings.SerializeTo(plainBytes);
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool isDisposing) {
        if (isDisposing) {
            storageSettings.Dispose();
            IsDisposed = true;
        }
    }
}
