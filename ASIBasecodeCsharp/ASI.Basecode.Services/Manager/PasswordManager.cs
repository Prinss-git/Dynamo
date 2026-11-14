using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ASI.Basecode.Services.Manager
{
    /// <summary>
    /// Password storage. New passwords are stored as salted one-way hashes (ASP.NET Core's PasswordHasher:
    /// PBKDF2 with HMAC-SHA512, 100,000 iterations). Accounts created by the original template or the demo data
    /// still hold AES-encrypted passwords; those are accepted once and replaced by a hash at the next sign-in.
    /// </summary>
    public class PasswordManager
    {
        private static readonly PasswordHasher<object> Hasher = new PasswordHasher<object>();

        /// <summary>
        /// Secret key of the legacy AES format (TokenAuthentication:SecretKey).
        /// </summary>
        public static string secretKey { get; set; }

        /// <summary>
        /// Sets up.
        /// </summary>
        /// <param name="tokenAuthConfig">Token authentication configuration</param>
        public static void SetUp(IConfigurationSection tokenAuthConfig)
        {
            secretKey = tokenAuthConfig.GetValue<string>("SecretKey");
        }

        /// <summary>
        /// Hashes a password for storage. Each call uses a new random salt, so equal passwords look different.
        /// </summary>
        public static string HashPassword(string password)
        {
            return Hasher.HashPassword(null, password);
        }

        /// <summary>
        /// Checks a typed password against the stored value (a hash, or the legacy AES format).
        /// <paramref name="needsUpgrade"/> is true when the stored value should be replaced by a fresh hash.
        /// </summary>
        public static bool VerifyPassword(string stored, string password, out bool needsUpgrade)
        {
            needsUpgrade = false;
            if (string.IsNullOrEmpty(stored) || password == null) return false;

            if (IsHash(stored))
            {
                var result = Hasher.VerifyHashedPassword(null, stored, password);
                needsUpgrade = result == PasswordVerificationResult.SuccessRehashNeeded;
                return result != PasswordVerificationResult.Failed;
            }

            var matches = string.Equals(stored, LegacyEncrypt(password), StringComparison.Ordinal);
            needsUpgrade = matches;
            return matches;
        }

        public static bool VerifyPassword(string stored, string password)
        {
            return VerifyPassword(stored, password, out _);
        }

        /// <summary>
        /// A short fingerprint of the stored password. It goes into the sign-in cookie, so a password change or
        /// reset ends every other session of that account.
        /// </summary>
        public static string Stamp(string stored)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(stored ?? string.Empty));
            return Convert.ToHexString(bytes, 0, 8);
        }

        /// <summary>PasswordHasher output: Base64 of a format marker byte (0x01 = v3) followed by salt and subkey.</summary>
        private static bool IsHash(string stored)
        {
            if (stored.Length < 40) return false;   // the legacy format is 24-44 characters of AES output
            try
            {
                var bytes = Convert.FromBase64String(stored);
                return bytes.Length > 13 && bytes[0] == 0x01;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// The original template's reversible AES encryption (fixed key and IV). Only used to recognize old
        /// stored passwords; Database/build_hosted_script.ps1 produces the same values for the demo data.
        /// </summary>
        private static string LegacyEncrypt(string password)
        {
            byte[] iv = new byte[16];
            byte[] aesKey = new byte[16];

            using (Aes aes = Aes.Create())
            {
                var keyBytes = Encoding.UTF8.GetBytes(secretKey ?? string.Empty);
                Array.Copy(keyBytes, aesKey, Math.Min(keyBytes.Length, aesKey.Length));
                aes.Key = aesKey;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(password);
                    }
                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }
    }
}
