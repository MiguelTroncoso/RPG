using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace MmorpgPrototype
{
    // Local-only account gate for the offline beta. It intentionally stores a
    // password hash, never the password itself. Online accounts must be owned
    // by the backend when the public server phase begins.
    public static class OfflineAccountStore
    {
        private const string AccountPrefix = "mmorpg.offline.account.";
        private const string SessionAccountKey = "mmorpg.offline.session.account";
        private const string PasswordSalt = "ValleDeLasReliquias.OfflineBeta.v1";

        public static string CurrentAccount => PlayerPrefs.GetString(SessionAccountKey, string.Empty);
        public static bool HasActiveSession => !string.IsNullOrWhiteSpace(CurrentAccount);
        public static string CurrentStorageKey => Hash(CurrentAccount).Substring(0, 16);

        public static bool TryRegister(string accountName, string password, out string message)
        {
            var account = Normalize(accountName);
            if (!IsValidAccount(account))
            {
                message = "Usa entre 3 y 16 caracteres: letras, numeros o guion bajo.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                message = "La contrasena debe tener al menos 6 caracteres.";
                return false;
            }

            var key = AccountKey(account);
            if (PlayerPrefs.HasKey(key))
            {
                message = "Esa cuenta ya existe. Ingresa con tu contrasena.";
                return false;
            }

            PlayerPrefs.SetString(key, PasswordHash(account, password));
            PlayerPrefs.SetString(SessionAccountKey, account);
            PlayerPrefs.Save();
            message = "Cuenta creada en este dispositivo.";
            return true;
        }

        public static bool TrySignIn(string accountName, string password, out string message)
        {
            var account = Normalize(accountName);
            var key = AccountKey(account);
            if (!PlayerPrefs.HasKey(key))
            {
                message = "No existe una cuenta local con ese nombre.";
                return false;
            }

            if (!string.Equals(PlayerPrefs.GetString(key), PasswordHash(account, password), StringComparison.Ordinal))
            {
                message = "Contrasena incorrecta.";
                return false;
            }

            PlayerPrefs.SetString(SessionAccountKey, account);
            PlayerPrefs.Save();
            message = "Sesion local iniciada.";
            return true;
        }

        public static void SignOut()
        {
            PlayerPrefs.DeleteKey(SessionAccountKey);
            PlayerPrefs.Save();
        }

        private static bool IsValidAccount(string account)
        {
            if (account.Length < 3 || account.Length > 16)
            {
                return false;
            }

            foreach (var character in account)
            {
                if (!(char.IsLetterOrDigit(character) || character == '_'))
                {
                    return false;
                }
            }

            return true;
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static string AccountKey(string account)
        {
            return AccountPrefix + Hash(account).Substring(0, 24);
        }

        private static string PasswordHash(string account, string password)
        {
            return Hash($"{PasswordSalt}|{account}|{password ?? string.Empty}");
        }

        private static string Hash(string value)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (var item in bytes)
                {
                    builder.Append(item.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
