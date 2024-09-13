using System.IO;
using System.Text;

namespace VDT.Lock.Tests;

public static class StringExtensions {
    public static Stream ToStream(this string str) {
        var stream = new MemoryStream();
        var bytes = Encoding.UTF8.GetBytes(str);

        stream.Write(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }
}
