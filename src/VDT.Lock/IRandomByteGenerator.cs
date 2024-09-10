namespace VDT.Lock {
    public interface IRandomByteGenerator {
        byte[] Generate(int count);
    }
}