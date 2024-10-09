using System;

namespace VDT.Lock;

public sealed class DataItem : IDisposable {
    private SecureBuffer plainNameBuffer;
    
    public ReadOnlySpan<byte> Name {
        get => new(plainNameBuffer.Value);
        set {
            plainNameBuffer.Dispose();
            plainNameBuffer = new(value.ToArray());
        }
    }

    public DataCollection<DataField> Fields { get; } = new();

    public DataItem() : this(ReadOnlySpan<byte>.Empty) { }

    public DataItem(ReadOnlySpan<byte> plainNameSpan) {
        plainNameBuffer = new(plainNameSpan.ToArray());
    }

    public void Dispose() {
        plainNameBuffer.Dispose();
        Fields.Dispose();
        GC.SuppressFinalize(this);
    }
}
