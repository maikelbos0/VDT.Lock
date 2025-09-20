using System;

namespace VDT.Lock;

public sealed class SaveDataStoreResult : IDisposable {
    private readonly DataCollection<DataValue> succeededStorageSites = [];
    private readonly DataCollection<DataValue> failedStorageSites = [];

    public bool IsDisposed { get; private set; }

    public DataCollection<DataValue> SucceededStorageSites {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return succeededStorageSites;
        }
    }

    public DataCollection<DataValue> FailedStorageSites {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return failedStorageSites;
        }
    }

    public void Dispose() {
        succeededStorageSites.Dispose();
        failedStorageSites.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
