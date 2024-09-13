using System.IO;
using System.Text;

namespace VDT.Lock.Tests;

public static class StringExtensions { 
    public static Stream ToStream(this string str) => Encoding.UTF8.GetBytes(str).ToStream();
}
