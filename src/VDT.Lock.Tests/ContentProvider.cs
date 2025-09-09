using System;
using System.IO;
using System.Reflection;

namespace VDT.Lock.Tests;

public static class ContentProvider {
    private static readonly string contentLocation = Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(),
        "Content"
    );

    public static string GetFilePath(string fileName)
        => Path.Combine(contentLocation, fileName);

    public static byte[] GetFileContents(string fileName)
        => File.ReadAllBytes(GetFilePath(fileName));
}
