using System;
using System.Security.Cryptography;
using System.Text;

namespace Mahou {
    internal static class SecretProtector {
        internal const string Prefix = "dpapi:";
        static readonly byte[] Entropy = Encoding.UTF8.GetBytes("MIXANIZM-Mahou-ProxyPassword-v1");

        internal static string Protect(string value) {
            if (String.IsNullOrEmpty(value)) return String.Empty;
            if (value.StartsWith(Prefix, StringComparison.Ordinal)) return value;
            var plain = Encoding.UTF8.GetBytes(value);
            var encrypted = ProtectedData.Protect(plain, Entropy, DataProtectionScope.CurrentUser);
            return Prefix + Convert.ToBase64String(encrypted);
        }

        internal static bool TryUnprotect(string value, out string plainText) {
            plainText = String.Empty;
            if (String.IsNullOrEmpty(value)) return true;
            if (!value.StartsWith(Prefix, StringComparison.Ordinal)) return false;
            try {
                var encrypted = Convert.FromBase64String(value.Substring(Prefix.Length));
                var plain = ProtectedData.Unprotect(encrypted, Entropy, DataProtectionScope.CurrentUser);
                plainText = Encoding.UTF8.GetString(plain);
                return true;
            } catch {
                return false;
            }
        }

        internal static bool TryDecodeLegacyBase64(string value, out string plainText) {
            plainText = String.Empty;
            if (String.IsNullOrEmpty(value)) return true;
            try {
                var bytes = Convert.FromBase64String(value);
                plainText = Encoding.Unicode.GetString(bytes);
                return true;
            } catch {
                return false;
            }
        }
    }
}
