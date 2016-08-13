using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace WinCrypt
{
  public static class FileExtra
  {
    public static void SecureDelete(string path, int timesToWrite = 10)
    {
      if (File.Exists(path))
      {
        File.SetAttributes(path, FileAttributes.Normal);

        double sectors = Math.Ceiling(new FileInfo(path).Length / 512.0);

        byte[] dummyBuffer = new byte[512];

        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
          using (FileStream inputStream = new FileStream(path, FileMode.Open))
          {
            for (int currentPass = 0; currentPass < timesToWrite; currentPass++)
            {

              inputStream.Position = 0;

              for (int sectorsWritten = 0; sectorsWritten < sectors; sectorsWritten++)
              {
                rng.GetBytes(dummyBuffer);
                inputStream.Write(dummyBuffer, 0, dummyBuffer.Length);
              }
            }
            inputStream.SetLength(0);
            inputStream.Close();
          }
        }

        DateTime dt = new DateTime(2037, 1, 1, 0, 0, 0);
        File.SetCreationTime(path, dt);
        File.SetLastAccessTime(path, dt);
        File.SetLastWriteTime(path, dt);

        File.SetCreationTimeUtc(path, dt);
        File.SetLastAccessTimeUtc(path, dt);
        File.SetLastWriteTimeUtc(path, dt);

        File.Delete(path);
      }
    }

    public static string GetUniqueFilePath(string path)
    {
      if (File.Exists(path))
      {
        string folder = Path.GetDirectoryName(path);
        string filename = Path.GetFileNameWithoutExtension(path);
        string extension = Path.GetExtension(path);
        int number = 1;

        Match regex = Regex.Match(path, @"(.+) \((\d+)\)\.\w+");

        if (regex.Success)
        {
          filename = regex.Groups[1].Value;
          number = int.Parse(regex.Groups[2].Value);
        }

        do
        {
          number++;
          path = Path.Combine(folder, string.Format("{0} ({1}){2}", filename, number, extension));
        }
        while (File.Exists(path));
      }

      return path;
    }
  }
}