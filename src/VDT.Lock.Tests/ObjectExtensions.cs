using System;
using System.Reflection;

namespace VDT.Lock.Tests;

public static class ObjectExtensions {
    public static byte[] GetBufferValue(this object obj, string fieldName = "buffer") {
        var fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();
        var buffer = fieldInfo.GetValue(obj) as SecureBuffer ?? throw new InvalidOperationException();

        return buffer.Value;
    }
}
