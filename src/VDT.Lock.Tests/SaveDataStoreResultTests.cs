using System;
using Xunit;

namespace VDT.Lock.Tests;

public class SaveDataStoreResultTests {
    [Fact]
    public void Dispose() {
        DataCollection<DataValue> succeededStorageSites;
        DataCollection<DataValue> failedStorageSites;

        using (var subject = new SaveDataStoreResult()) {
            succeededStorageSites = subject.SucceededStorageSites;
            failedStorageSites = subject.FailedStorageSites;
        }

        Assert.True(succeededStorageSites.IsDisposed);
        Assert.True(failedStorageSites.IsDisposed);
    }

    [Fact]
    public void IsDisposed() {
        SaveDataStoreResult disposedSubject;

        using (var subject = new SaveDataStoreResult()) {
            disposedSubject = subject;
        }

        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void SucceededStorageSitesThrowsIfDisposed() {
        SaveDataStoreResult disposedSubject;

        using (var subject = new SaveDataStoreResult()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.SucceededStorageSites; });
    }

    [Fact]
    public void FailedStorageSitesThrowsIfDisposed() {
        SaveDataStoreResult disposedSubject;

        using (var subject = new SaveDataStoreResult()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.FailedStorageSites; });
    }
}
