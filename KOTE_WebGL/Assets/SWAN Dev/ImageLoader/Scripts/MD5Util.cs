
using System;
using System.Security.Cryptography;
using System.Text;

namespace IMBX
{
    public static class MD5Util
    {
        public static string ToMD5Hash(string input)
        {
            MD5 md5 = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new StringBuilder to collect the bytes and create a string.
            StringBuilder builder = new StringBuilder();

            // Loop through each byte of the hashed data and format each one as a hexadecimal strings.
            for (int i = 0; i < hash.Length; i++) builder.Append(hash[i].ToString("x2"));

            // Return the hexadecimal string
            return builder.ToString().ToUpper();
        }

        public static bool VerifyMD5(string input, string hash)
        {
            // Hash the input
            string hashOfInput = ToMD5Hash(input);

            // Create a StringComparer to compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return 0 == comparer.Compare(hashOfInput, hash);
        }
    }
}
