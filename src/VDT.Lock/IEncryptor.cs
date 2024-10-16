﻿using System.Threading.Tasks;

namespace VDT.Lock {
    public interface IEncryptor {
        Task<SecureBuffer> Encrypt(SecureBuffer plainBuffer, SecureBuffer keyBuffer);
        Task<SecureBuffer> Decrypt(SecureBuffer encryptedBuffer, SecureBuffer keyBuffer);
    }
}
