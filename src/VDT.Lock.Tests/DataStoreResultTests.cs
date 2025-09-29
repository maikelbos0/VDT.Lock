using System;
using Xunit;

namespace VDT.Lock.Tests;

public class DataStoreResultTests {
    [Fact]
    public void Dispose() {
        DataStoreResult subject;
        DataCollection<DataValue> succeededStorageSites;
        DataCollection<DataValue> failedStorageSites;

        using (subject = new()) {
            succeededStorageSites = subject.SucceededStorageSites;
            failedStorageSites = subject.FailedStorageSites;
        }

        Assert.True(subject.IsDisposed);
        Assert.True(succeededStorageSites.IsDisposed);
        Assert.True(failedStorageSites.IsDisposed);
    }

    [Fact]
    public void SucceededStorageSitesThrowsIfDisposed() {
        DataStoreResult disposedSubject;

        using (var subject = new DataStoreResult()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.SucceededStorageSites; });
    }

    [Fact]
    public void FailedStorageSitesThrowsIfDisposed() {
        DataStoreResult disposedSubject;

        using (var subject = new DataStoreResult()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.FailedStorageSites; });
    }
}
