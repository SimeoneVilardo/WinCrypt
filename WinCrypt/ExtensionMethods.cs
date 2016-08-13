using System;

namespace WinCrypt
{
  public static class ExtensionMethods
  {
    public static byte[] GetBytes(this string str)
    {
      byte[] bytes = new byte[str.Length * sizeof(char)];
      Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
      return bytes;
    }

    public static string GetString(this byte[] bytes)
    {
      char[] chars = new char[bytes.Length / sizeof(char)];
      Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
      return new string(chars);
    }

    public static byte[] GetBytes(this int intValue)
    {
      byte[] bytes = BitConverter.GetBytes(intValue);
      if (BitConverter.IsLittleEndian)
        Array.Reverse(bytes);
      return bytes;
    }

    public static int GetInteger(this byte[] bytes)
    {
      if (BitConverter.IsLittleEndian)
        Array.Reverse(bytes);
      int intValue = BitConverter.ToInt32(bytes, 0);
      return intValue;
    }
  }
}