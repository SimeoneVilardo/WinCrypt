using System;

namespace WinCrypt
{
  public class Program
  {
    private static string path = string.Empty;
    private static string password = string.Empty;

    private static string[] getAllFiles(string path)
    {
      System.Collections.Generic.List<string> files = new System.Collections.Generic.List<string>();
      foreach (string file in System.IO.Directory.EnumerateFiles(
        path, "*.*", System.IO.SearchOption.AllDirectories))
      {
        files.Add(file);
      }
      return files.ToArray();
    }

    private static void getParams()
    {
      if (string.IsNullOrWhiteSpace(path))
      {
        Console.Write("Inserisci il percorso del file: ");
        path = Console.ReadLine();
      }
      if (string.IsNullOrWhiteSpace(password))
      {
        Console.Write("Inserisci la password: ");
        password = Console.ReadLine();
      }
    }

    public static void Main(string[] args)
    {
      byte command = 0;

      if (args.Length == 1)
      {
        path = args[0];
        if (path.EndsWith(".crypt"))
        {
          command = 2;
        }
      }
      else if (args.Length == 2)
      {
        path = args[0];
        password = args[1];
      }

      while (true)
      {
        Console.WriteLine("WinCrypt Versione AES");
        if (command == 0)
        {
          Console.WriteLine("MENU");
          Console.WriteLine(" 1 - Codifica");
          Console.WriteLine(" 2 - Decodifica");
          Console.WriteLine(" 3 - Chiudi");
          Console.Write("Numero operazione: ");
          byte.TryParse(Console.ReadLine(), out command);
        }
        try
        {
          switch (command)
          {
            case 1:
              getParams();
              if (System.IO.File.GetAttributes(path).HasFlag(System.IO.FileAttributes.Directory))
              {
                string[] files = getAllFiles(path);
                foreach (string file in files)
                {
                  try
                  {
                    Cipher.Encrypt(file, password);
                  }
                  catch (Exception encEx)
                  {
                    Console.WriteLine("Errore nella codfica del file {0}: {1}", System.IO.Path.GetFileName(file), encEx.Message);
                  }
                }
              }
              else
              {
                Cipher.Encrypt(path, password);
              }
              break;
            case 2:
              getParams();
              if (System.IO.File.GetAttributes(path).HasFlag(System.IO.FileAttributes.Directory))
              {
                string[] files = getAllFiles(path);
                foreach (string file in files)
                {
                  try
                  {
                    Cipher.Decrypt(file, password);
                  }
                  catch (Exception decEx)
                  {
                    Console.WriteLine("Errore nella decodfica del file {0}: {1}", System.IO.Path.GetFileName(file), decEx.Message);
                  }
                }
              }
              else
              {
                Cipher.Decrypt(path, password);
              }
              break;
            case 3:
              Environment.Exit(0);
              break;
          }
          Console.WriteLine("Operazione completata");
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
        Console.WriteLine("Premi INVIO per continuare...");
        Console.ReadLine();
        Console.WriteLine();
        Console.WriteLine();
        command = 0;
        path = string.Empty;
        password = string.Empty;
      }
    }
  }
}