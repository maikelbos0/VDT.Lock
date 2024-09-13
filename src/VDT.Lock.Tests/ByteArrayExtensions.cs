using System.IO;

namespace VDT.Lock.Tests;

public static class ByteArrayExtensions {
    public static Stream ToStream(this byte[] bytes) {
        var stream = new MemoryStream();

        stream.Write(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }
}
