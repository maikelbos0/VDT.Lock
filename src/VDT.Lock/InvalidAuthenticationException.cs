using System;

namespace VDT.Lock;

public class InvalidAuthenticationException : Exception {
    public InvalidAuthenticationException() { }

    public InvalidAuthenticationException(string? message) : base(message) { }

    public InvalidAuthenticationException(string? message, Exception innerException) : base(message, innerException) { }
}
