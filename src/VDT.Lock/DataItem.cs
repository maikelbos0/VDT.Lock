using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VDT.Lock;

public sealed class DataItem : IDisposable {
    private SecureBuffer plainNameBuffer;
    private readonly List<DataField> fields = [];

    //public IList<string> Tags { get; set; }
    //public IList<string> Locations { get; set; }

    public ReadOnlySpan<byte> Name {
        get => new(plainNameBuffer.Value);
        set {
            plainNameBuffer.Dispose();
            plainNameBuffer = new(value.ToArray());
        }
    }

    public ReadOnlyCollection<DataField> Fields {
        get {
            return new ReadOnlyCollection<DataField>(fields.ToArray());
        }
    }

    public DataItem(ReadOnlySpan<byte> plainNameSpan) {
        plainNameBuffer = new(plainNameSpan.ToArray());
    }

    public void Dispose() {
        plainNameBuffer.Dispose();
        foreach (var field in fields) {
            field.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
