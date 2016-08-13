namespace WinCrypt
{
  public class Constants
  {
    public struct AES_OPTIONS
    {
      public const int KEY_SIZE = 256;
      public const int BLOCK_SIZE = 128;
      public const int ITERATIONS = 10000;
    }
    public struct SIZES
    {
      public const int BUFFER_SIZE = 2048;
      public const int SALT_SIZE = 2048;
      public const int FILENAME_LENGTH_SIZE = 4;
      public const int SALT_LENGTH_SIZE = 4;
    }
  }
}
