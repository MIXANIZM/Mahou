using System;
using System.Security.Cryptography;
using System.Text;

namespace Mahou
{
    internal static class SecretProtector
    {
        private const string Prefix = "dpapi:";

        public static bool IsProxyPassword(string section, string key)
        {
            return String.Equals(section, "Proxy", StringComparison.OrdinalIgnoreCase) &&
                String.Equals(key, "Password", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsProtectedValue(string value)
        {
            return !String.IsNullOrEmpty(value) && value.StartsWith(Prefix, StringComparison.Ordinal);
        }

        public static string Protect(string value)
        {
            if (String.IsNullOrEmpty(value) || IsProtectedValue(value))
                return value ?? String.Empty;

            var plaintext = Encoding.UTF8.GetBytes(value);
            var encrypted = ProtectedData.Protect(plaintext, null, DataProtectionScope.CurrentUser);
            return Prefix + Convert.ToBase64String(encrypted);
        }

        public static bool TryUnprotect(string value, out string plaintext)
        {
            plaintext = String.Empty;
            if (!IsProtectedValue(value))
                return false;

            try
            {
                var encrypted = Convert.FromBase64String(value.Substring(Prefix.Length));
                var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                plaintext = Encoding.UTF8.GetString(decrypted);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
