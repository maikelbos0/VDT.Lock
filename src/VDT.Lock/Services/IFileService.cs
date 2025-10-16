namespace VDT.Lock.Services;

public interface IFileService {
    byte[] ReadAllBytes(string path);

    void WriteAllBytes(string path, byte[] contents);
}