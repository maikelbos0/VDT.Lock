using System.Collections.ObjectModel;

namespace VDT.Lock;

public sealed class DataItem : IDisposable {
    private SecureBuffer plainNameBuffer;
    private readonly object plainNameBufferLock = new();
    private readonly List<DataField> fields = [];
    private readonly object fieldsLock = new();


    //public IList<string> Tags { get; set; }
    //public IList<string> Locations { get; set; }

    public ReadOnlySpan<byte> Name {
        get => new(plainNameBuffer.Value);
        set {
            lock (plainNameBufferLock) {
                plainNameBuffer.Dispose();
                plainNameBuffer = new(value.ToArray());
            }
        }
    }

    public ReadOnlyCollection<DataField> Fields {
        get {
            lock (fieldsLock) {
                return new ReadOnlyCollection<DataField>(fields.ToArray());
            }
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
