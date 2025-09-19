using System;

namespace VDT.Lock;

public class SecureBufferSplitter {
    private readonly SecureBuffer buffer;
    private readonly int sectionSize;

    public int SectionCount { get; }

    public SecureBufferSplitter(SecureBuffer buffer, int sectionSize) {
        this.buffer = buffer;
        this.sectionSize = sectionSize;
        SectionCount = (this.buffer.Value.Length - 1) / sectionSize + 1;
    }

    public byte[] GetHeader() {
        unchecked {
            return [(byte)SectionCount, (byte)(SectionCount >> 8), (byte)(SectionCount >> 16), (byte)(SectionCount >> 24)];
        }
    }

    public ReadOnlySpan<byte> GetSection(int sectionIndex)
        => new(buffer.Value, sectionIndex * sectionSize, Math.Min(sectionSize, buffer.Value.Length - sectionIndex * sectionSize));
}
