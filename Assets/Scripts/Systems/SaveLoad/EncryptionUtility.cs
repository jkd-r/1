using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ProtocolEMR.Systems.SaveLoad
{
    /// <summary>
    /// Provides AES encryption helpers for save data.
    /// </summary>
    public static class EncryptionUtility
    {
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public static byte[] DeriveKey(string secret, string profileId)
        {
            string seed = string.IsNullOrEmpty(profileId) ? secret : $"{secret}:{profileId}";
            return DeriveBytes(seed, 32);
        }

        public static byte[] DeriveIV(string secret, string slotToken)
        {
            string seed = string.IsNullOrEmpty(slotToken) ? secret : $"{slotToken}:{secret}";
            return DeriveBytes(seed, 16);
        }

        public static string ComputeHash(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(data);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }

        private static byte[] DeriveBytes(string input, int length)
        {
            using var sha = SHA256.Create();
            byte[] seed = Encoding.UTF8.GetBytes(input ?? string.Empty);
            byte[] hash = sha.ComputeHash(seed);

            if (length == hash.Length)
                return hash;

            if (length < hash.Length)
                return hash.Take(length).ToArray();

            byte[] result = new byte[length];
            int copied = 0;
            while (copied < length)
            {
                int toCopy = Math.Min(hash.Length, length - copied);
                Array.Copy(hash, 0, result, copied, toCopy);
                copied += toCopy;
                if (copied < length)
                {
                    hash = sha.ComputeHash(hash);
                }
            }

            return result;
        }
    }
}
