using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetFox.Core.TestHelpers { 
    public static class RandomStrings
    {
        public static string Length11()
        {
            var path = Path.GetRandomFileName();
            return path.Replace(".", "");
        }

        public static string WithPrefix(string prefix)
        {
            var suffix = Length11();
            return $"{prefix}{suffix}";
        }

        private static Random Random = new Random();

        private const string AllowedChars = "0123456789abcdefghijklmnopqrstuvwxyz";
        /// <summary>Not guaranteed to be unique. But if not called too often,
        /// will be unique most of the time.</summary> 
        public static string WithPrefixAnd2CharSuffix(string prefix)
        {
            var length = AllowedChars.Length;
            var suffixLength = 2;
            var suffix = "";
            for (int i = 0; i< suffixLength; i++)
            {
                var random = Random.Next(length);
                var randomChar = AllowedChars[random];
                suffix = suffix + randomChar;
            }
            var r = prefix + suffix;
            return r;
        }
    }
}
