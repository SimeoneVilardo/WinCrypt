using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WinCrypt
{
    public static class Cipher
    {
        public static void Encrypt(string path, string password)
        {
            byte[] saltBytes = getSalt(Constants.SIZES.SALT_SIZE);
            int saltLength = saltBytes.Length;
            byte[] saltLengthBytes = saltLength.GetBytes();

            string filename = Path.GetFileName(path);
            byte[] filenameBytes = filename.GetBytes();
            int filenameLength = filenameBytes.Length;
            byte[] filenameLengthBytes = filenameLength.GetBytes();

            byte[] passwordBytes = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(password));

            string tempPath = Path.Combine(Path.GetDirectoryName(path), string.Format("{0}_{1}_{2}.{3}", Path.GetFileNameWithoutExtension(path), "encrypted", DateTime.Now.ToString("ddMMyyyyHHmmssfff"), "temp"));

            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, Constants.AES_OPTIONS.ITERATIONS);
            using (RijndaelManaged AES = new RijndaelManaged() { Mode = CipherMode.CBC, KeySize = Constants.AES_OPTIONS.KEY_SIZE, BlockSize = Constants.AES_OPTIONS.BLOCK_SIZE, Key = key.GetBytes(Constants.AES_OPTIONS.KEY_SIZE / 8), IV = key.GetBytes(Constants.AES_OPTIONS.BLOCK_SIZE / 8) })
            {
                using (FileStream sourceStream = File.OpenRead(path))
                {
                    using (FileStream destinationStream = File.OpenWrite(tempPath))
                    {
                        destinationStream.Write(saltLengthBytes, 0, Constants.SIZES.SALT_LENGTH_SIZE);
                        destinationStream.Write(saltBytes, 0, Constants.SIZES.SALT_SIZE);
                        using (CryptoStream cryptoStream = new CryptoStream(destinationStream, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(filenameLengthBytes, 0, Constants.SIZES.FILENAME_LENGTH_SIZE);
                            cryptoStream.Write(filenameBytes, 0, filenameBytes.Length);

                            byte[] buffer = new byte[Constants.SIZES.BUFFER_SIZE];
                            int bytesRead = 0;
                            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                cryptoStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
            }

            string encryptedPath = FileExtra.GetUniqueFilePath(Path.ChangeExtension(path, "crypt"));
            FileExtra.SecureDelete(path);
            File.Move(tempPath, encryptedPath);
        }

        public static void Decrypt(string path, string password)
        {
            using (FileStream sourceStream = File.OpenRead(path))
            {
                byte[] saltLengthBytes = new byte[Constants.SIZES.SALT_LENGTH_SIZE];
                sourceStream.Read(saltLengthBytes, 0, Constants.SIZES.SALT_LENGTH_SIZE);
                int saltLength = saltLengthBytes.GetInteger();

                byte[] saltBytes = new byte[saltLength];
                sourceStream.Read(saltBytes, 0, saltLength);

                byte[] passwordBytes = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(password));

                string tempPath = Path.Combine(Path.GetDirectoryName(path), string.Format("{0}_{1}_{2}.{3}", Path.GetFileNameWithoutExtension(path), "decrypted", DateTime.Now.ToString("ddMMyyyyHHmmssfff"), "temp"));
                string filename = string.Empty;
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, Constants.AES_OPTIONS.ITERATIONS);
                using (RijndaelManaged AES = new RijndaelManaged() { Mode = CipherMode.CBC, KeySize = Constants.AES_OPTIONS.KEY_SIZE, BlockSize = Constants.AES_OPTIONS.BLOCK_SIZE, Key = key.GetBytes(Constants.AES_OPTIONS.KEY_SIZE / 8), IV = key.GetBytes(Constants.AES_OPTIONS.BLOCK_SIZE / 8) })
                {
                    try
                    {
                        using (FileStream destinationStream = File.OpenWrite(tempPath))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(sourceStream, AES.CreateDecryptor(), CryptoStreamMode.Read))
                            {
                                byte[] filenameLengthBytes = new byte[Constants.SIZES.FILENAME_LENGTH_SIZE];
                                cryptoStream.Read(filenameLengthBytes, 0, Constants.SIZES.FILENAME_LENGTH_SIZE);
                                int filenameLength = filenameLengthBytes.GetInteger();

                                byte[] filenameBytes = new byte[filenameLength];
                                cryptoStream.Read(filenameBytes, 0, filenameBytes.Length);
                                filename = filenameBytes.GetString();

                                byte[] buffer = new byte[Constants.SIZES.BUFFER_SIZE];
                                int bytesRead = 0;
                                while ((bytesRead = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    destinationStream.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        try
                        {
                            File.Delete(tempPath);
                        }
                        catch (Exception ioex)
                        {
                            //OK
                        }
                        throw ex;
                    }
                }

                string decryptedPath = Path.Combine(Path.GetDirectoryName(path), filename);
                File.Move(tempPath, decryptedPath);
                File.Delete(path);
            }
        }

        private static byte[] getSalt(int length)
        {
            byte[] salt = new byte[length];
            RandomNumberGenerator.Create().GetBytes(salt);
            return salt;
        }
    }
}