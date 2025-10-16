using System.IO;

namespace VDT.Lock.Services;

public class FileService : IFileService {
    public byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);

    public void WriteAllBytes(string path, byte[] contents) => File.WriteAllBytes(path, contents);
}
