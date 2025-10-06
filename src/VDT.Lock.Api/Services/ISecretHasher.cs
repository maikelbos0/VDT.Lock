namespace VDT.Lock.Api.Services;

public interface ISecretHasher {
    (byte[] salt, byte[] hash) HashSecret(byte[] secret);
}