using System;
using System.Linq;

namespace ValorantCC.src.Community
{
    public class SCGen
    {
        private static Random rand = new Random();
        public static string GenerateShareCode(string input)
        {
            input = input.Replace("-", String.Empty);

            return new string(
                    Enumerable.Repeat(input, 12)
                    .Select(s => s[rand.Next(s.Length)])
                    .ToArray()).ToUpper();
        }
    }
}
