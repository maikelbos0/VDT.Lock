using System;
using System.Collections.Generic;
using Xunit;

namespace VDT.Lock.Tests;

public class DataCollectionTests {
    public class TestDisposable : IDisposable {
        public bool IsDisposed { get; private set; }

        public void Dispose() {
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }
    }

    [Fact]
    public void Count() {
        using var subject = new DataCollection<TestDisposable>();

        subject.Add();

        Assert.Equal(1, subject.Count);
    }

    [Fact]
    public void Add() {
        using var subject = new DataCollection<TestDisposable>();

        var result = subject.Add();

        Assert.Equal(result, Assert.Single(subject));
    }

    [Fact]
    public void Dispose() {
        var items = new List<TestDisposable>();

        using (var subject = new DataCollection<TestDisposable>()) {
            items.Add(subject.Add());
            items.Add(subject.Add());
        };

        Assert.Equal(2, items.Count);

        foreach (var item in items) {
            Assert.True(item.IsDisposed);
        }
    }
}
