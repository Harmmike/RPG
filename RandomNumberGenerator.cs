using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Engine
{
    public static class RandomNumberGenerator
    {
        private static readonly RNGCryptoServiceProvider _generator = new RNGCryptoServiceProvider();

        public static int NumberBetween(int minValue, int maxValue) //non-deterministic version of random number generator *complicated*
        {
            byte[] randomNumber = new byte[1];

            _generator.GetBytes(randomNumber);

            double asciiValueOfRandomCharacter = Convert.ToDouble(randomNumber[0]);

            //Using Math.Max, and subracting 0.00000000001,
            //to ensure "multiplier" will always be between 0.0 and .9999999999
            //Otherwise, it's possible for it to be "1", which causes problems in rounding.
            double multiplier = Math.Max(0, (asciiValueOfRandomCharacter / 255d) - 0.00000000001d);

            //Add one to the range, to allow for the rounding done with Math.Floor
            int range = maxValue - minValue + 1;

            double randomValueInRange = Math.Floor(multiplier * range);

            return (int)(minValue + randomValueInRange);
        }

        private static readonly Random _simpleGenerator = new Random(); //deterministic version of rng. *simple version*
        public static int SimpleNumberBetween(int minValue, int maxValue)
        {
            return _simpleGenerator.Next(minValue, maxValue + 1);
        }
    }
}
