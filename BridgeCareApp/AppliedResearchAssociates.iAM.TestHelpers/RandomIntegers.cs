using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppliedResearchAssociates.iAM.TestHelpers
{
    public static class RandomIntegers
    {
        private static Random _random = new Random();

        private static HashSet<int> ProvidedNumbers { get; set; } = new HashSet<int>();

        private static int NextInteger()
        {
            var n = _random.Next(100000000);
            if (n == 0)
            {
                n = 1;
            }
            return n;
        }

        public static int PositiveNotRepeated()
        {
            // If we allow this to go to ten million,
            // we run into trouble because double.ToString() can
            // go into exponential notation, resulting in failure to
            // match a number and its corresponding text value.
            var n = NextInteger();
            while (ProvidedNumbers.Contains(n))
            {
                n = NextInteger();
            }
            ProvidedNumbers.Add(n);
            return n;
        }
    }
}
