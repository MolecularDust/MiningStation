using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Mining_Station
{
    /// Provides extension methods that deal with string encryption/decryption and
    /// "SecureString" encapsulation.
    public static class SecurityExtensions
    {
        /// Specifies the data protection scope of the DPAPI.
        private const DataProtectionScope Scope = DataProtectionScope.CurrentUser;
        private static byte[] Entropy = Encoding.ASCII.GetBytes("CT9OkY1qyF43jt4A");

        /// Encrypts a given password and returns the encrypted data as a base64 string.
        /// "plainText" An unencrypted string that needs to be secured.
        /// A base64 encoded string that represents the encrypted binary data.
        /// This solution is not really secure as we are keeping strings in memory.
        /// If runtime protection is essential, "SecureString" should be used.
        /// "ArgumentNullException" If "plainText" is a null reference.
        public static string Encrypt(this string plainText)
        {
            if (plainText == null) throw new ArgumentNullException("plainText");

            //encrypt data
            var data = Encoding.Unicode.GetBytes(plainText);
            byte[] encrypted = ProtectedData.Protect(data, Entropy, Scope);

            //return as base64 string
            return Convert.ToBase64String(encrypted);
        }

        public static string Encrypt(this char[] charArray)
        {
            if (charArray.Length == 0) throw new ArgumentNullException("charArray");

            //encrypt data
            var data = Encoding.Unicode.GetBytes(charArray);
            byte[] encrypted = ProtectedData.Protect(data, Entropy, Scope);

            //return as base64 string
            return Convert.ToBase64String(encrypted);
        }


        /// Decrypts a given string.
        /// "cipher" A base64 encoded string that was created
        /// through the "Encrypt(string) extension methods.
        /// The decrypted string. Keep in mind that the decrypted string remains in memory
        /// and makes your application vulnerable per se. If runtime protection
        /// is essential, "SecureString" should be used.
        /// "ArgumentNullException">If "cipher" is a null reference.
        public static string Decrypt(this string cipher)
        {
            if (cipher == null) throw new ArgumentNullException("cipher");

            //parse base64 string
            byte[] data = Convert.FromBase64String(cipher);

            //decrypt data
            byte[] decrypted = ProtectedData.Unprotect(data, Entropy, Scope);
            return Encoding.Unicode.GetString(decrypted);
        }

        /// Encrypts the contents of a secure string.
        /// "value" An unencrypted string that needs to be secured.
        /// A base64 encoded string that represents the encrypted binary data.
        /// "ArgumentNullException" If is a null reference.
        public static string Encrypt(this SecureString value)
        {
            if (value == null) throw new ArgumentNullException("value");

            IntPtr ptr = Marshal.SecureStringToCoTaskMemUnicode(value);
            try
            {
                char[] buffer = new char[value.Length];
                Marshal.Copy(ptr, buffer, 0, value.Length);

                byte[] data = Encoding.Unicode.GetBytes(buffer);
                byte[] encrypted = ProtectedData.Protect(data, Entropy, Scope);

                //return as base64 string
                return Convert.ToBase64String(encrypted);
            }
            finally
            {
                Marshal.ZeroFreeCoTaskMemUnicode(ptr);
            }
        }

        /// Decrypts a base64 encrypted string and returns the decrpyted data
        /// wrapped into a "SecureString" instance
        /// "cipher" A base64 encoded string that was created through the "Encrypt(string)"
        /// "Encrypt(SecureString)" extension methods.
        /// The decrypted string, wrapped into a "SecureString" instance.
        /// "ArgumentNullException" If is a null reference.
        public static SecureString DecryptSecure(this string cipher)
        {
            if (cipher == null) throw new ArgumentNullException("cipher");

            //parse base64 string
            byte[] data = Convert.FromBase64String(cipher);

            //decrypt data
            byte[] decrypted = ProtectedData.Unprotect(data, Entropy, Scope);

            SecureString ss = new SecureString();

            //parse characters one by one - doesn't change the fact that
            //we have them in memory however...
            int count = Encoding.Unicode.GetCharCount(decrypted);
            int bc = decrypted.Length / count;
            for (int i = 0; i < count; i++)
            {
                ss.AppendChar(Encoding.Unicode.GetChars(decrypted, i * bc, bc)[0]);
            }
            //mark as read-only
            ss.MakeReadOnly();
            return ss;
        }

        /// Wraps a managed string into a "SecureString" instance.
        /// "value" A string or char sequence that should be encapsulated.
        /// A "SecureString" that encapsulates the submitted value.
        /// "ArgumentNullException" If is a null reference.
        public static SecureString ToSecureString(this IEnumerable<char> value)
        {
            if (value == null) throw new ArgumentNullException("value");

            var secured = new SecureString();

            var charArray = value.ToArray();
            for (int i = 0; i < charArray.Length; i++)
            {
                secured.AppendChar(charArray[i]);
            }

            secured.MakeReadOnly();
            return secured;
        }

        /// Unwraps the contents of a secured string and returns the contained value.
        /// "value" Be aware that the unwrapped managed string can be extracted from memory.
        /// "ArgumentNullException">If is a null reference.
        public static string Unwrap(this SecureString value)
        {
            if (value == null) throw new ArgumentNullException("value");

            IntPtr ptr = Marshal.SecureStringToCoTaskMemUnicode(value);
            try
            {
                return Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                Marshal.ZeroFreeCoTaskMemUnicode(ptr);
            }
        }

        /// Checks whether a "SecureString" is either  null or has a "SecureString.Length" of 0.
        /// "value" The secure string to be inspected.
        /// True if the string is either null or empty
        public static bool IsNullOrEmpty(this SecureString value)
        {
            return value == null || value.Length == 0;
        }

        /// Performs bytewise comparison of two secure strings: "value" and "other"
        /// True if the strings are equal.
        public static bool Matches(this SecureString value, SecureString other)
        {
            if (value == null && other == null) return true;
            if (value == null || other == null) return false;
            if (value.Length != other.Length) return false;
            if (value.Length == 0 && other.Length == 0) return true;

            IntPtr ptrA = Marshal.SecureStringToCoTaskMemUnicode(value);
            IntPtr ptrB = Marshal.SecureStringToCoTaskMemUnicode(other);
            try
            {
                //parse characters one by one - doesn't change the fact that
                //we have them in memory however...
                byte byteA = 1;
                byte byteB = 1;

                int index = 0;
                while (((char)byteA) != '\0' && ((char)byteB) != '\0')
                {
                    byteA = Marshal.ReadByte(ptrA, index);
                    byteB = Marshal.ReadByte(ptrB, index);
                    if (byteA != byteB) return false;
                    index += 2;
                }

                return true;
            }
            finally
            {
                Marshal.ZeroFreeCoTaskMemUnicode(ptrA);
                Marshal.ZeroFreeCoTaskMemUnicode(ptrB);
            }
        }
    }
}
