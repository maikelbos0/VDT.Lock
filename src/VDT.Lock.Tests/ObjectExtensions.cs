using System;
using System.Reflection;

namespace VDT.Lock.Tests;

public static class ObjectExtensions {
    public static SecureBuffer GetBuffer(this object obj, string fieldName = "buffer") {
        var fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();
        
        return fieldInfo.GetValue(obj) as SecureBuffer ?? throw new InvalidOperationException();
    }
}
