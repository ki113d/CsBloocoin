using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ki113d.CsBloocoin {

    /// <summary>
    /// Simple container object to represent a string and it's hashed value.
    /// </summary>
    class StringHashPair {

        public String str { get; set; }
        public String hash { get; set; }

        public StringHashPair( String str, String hash ) {
            this.str = str;
            this.hash = hash;
        }
    }

    class Util {
        private static Random rng = new Random((int)DateTime.Now.Ticks);
        private const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        /// <summary>
        /// Creates a new random string of size length.
        /// </summary>
        /// <param name="size">The expected length of the string.</param>
        /// <returns>A new random string of size length.</returns>
        public static string randomString( int size ) {
            char[] buffer = new char[size];

            for (int i = 0; i < size; i++) {
                buffer[i] = chars[rng.Next(chars.Length)];
            }
            return new string(buffer);
        }

        /// <summary>
        /// Generates a new SHA1 hash from a random string.
        /// </summary>
        /// <returns>A StringHashPair containing a string and it's hash</returns>
        public static StringHashPair sha1() {
            String randStr = randomString(20);
            SHA1 sha = new SHA1CryptoServiceProvider();
            Byte[] bytes = new Byte[20];
            Byte[] bHash = Encoding.ASCII.GetBytes(randStr);

            String hash = BitConverter.ToString(sha.ComputeHash(bHash)).Replace("-", "");
            return new StringHashPair(randStr, hash);
        }

        /// <summary>
        /// Generates a new SHA512 hash from a random string.
        /// </summary>
        /// <returns>A StringHashPair containing a string and it's hash</returns>
        public static StringHashPair sha512() {
            String randStr = randomString(20);
            SHA512 sha = new SHA512CryptoServiceProvider();
            Byte[] bytes = new Byte[20];
            Byte[] bHash = Encoding.ASCII.GetBytes(randStr);

            String hash = BitConverter.ToString(sha.ComputeHash(bHash)).Replace("-", "");
            return new StringHashPair(randStr, hash);
        }
    }
}
