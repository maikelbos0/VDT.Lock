using System.IO;

namespace VDT.Lock.Tests;

public static class StreamExtensions {
    public static byte[] ToArray(this Stream stream) {
        var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }
}
