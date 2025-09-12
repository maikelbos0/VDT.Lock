using System;
using System.Reflection;

namespace VDT.Lock.Tests;

public static class ObjectExtensions {
    public static SecureBuffer GetBuffer<T>(this T obj, string fieldName = "buffer") {
        var fieldInfo = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();
        
        return fieldInfo.GetValue(obj) as SecureBuffer ?? throw new InvalidOperationException();
    }
}
