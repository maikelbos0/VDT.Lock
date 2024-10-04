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

        var item = subject.Add();

        Assert.Equal(item, Assert.Single(subject));
    }

    [Fact]
    public void ContainsWhenPresent() {
        using var subject = new DataCollection<TestDisposable>();

        var item = subject.Add();

        Assert.True(subject.Contains(item));
    }

    [Fact]
    public void ContainsWhenNotPresent() {
        using var subject = new DataCollection<TestDisposable>();

        using var item = new TestDisposable();

        Assert.False(subject.Contains(item));
    }

    [Fact]
    public void RemoveWhenPresent() {
        using var subject = new DataCollection<TestDisposable>();

        var item = subject.Add();

        Assert.True(subject.Remove(item));
        Assert.True(item.IsDisposed);
    }

    [Fact]
    public void RemoveWhenNotPresent() {
        using var subject = new DataCollection<TestDisposable>();

        using var item = new TestDisposable();

        Assert.False(subject.Remove(item));
        Assert.False(item.IsDisposed);
    }

    [Fact]
    public void Clear() {
        var items = new List<TestDisposable>();
        using var subject = new DataCollection<TestDisposable>();

        items.Add(subject.Add());
        items.Add(subject.Add());
        subject.Clear();

        Assert.Equal(0, subject.Count);
        Assert.Equal(2, items.Count);

        foreach (var item in items) {
            Assert.True(item.IsDisposed);
        }
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
