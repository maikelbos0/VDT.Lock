using System.Security.Cryptography;

namespace VDT.Lock;

public sealed class RandomByteGenerator : IRandomByteGenerator {
    public byte[] Generate(int count) => RandomNumberGenerator.GetBytes(count);
}
