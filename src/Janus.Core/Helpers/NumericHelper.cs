using System;

namespace Janus.Core.Helpers
{
    public static class NumericHelper
    {
        public static int[] Primes => primes;
        private static int[] primes = {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369 };

        public const int HashPrime = 101;

        public static bool IsPrime(int number)
        {
            if ((number & 1) != 0)
            {
                var limit = (int)Math.Sqrt(number);
                for (var divisor = 3; divisor <= limit; divisor += 2)
                {
                    if ((number % divisor) == 0)
                        return false;
                }
                return true;
            }
            return (number == 2);
        }

        public static int ComputeNextPrime(int number)
        {
            for (var i = (number | 1); i < int.MaxValue; i += 2)
            {
                if (IsPrime(i) && ((i - 1) % HashPrime != 0))
                    return i;
            }
            return number;
        }

        public static int GetNextPrime(int number)
        {
            if (number < 0)
                throw new ArgumentException("Cannot be less than zero!", nameof(number));
            for (var i = 0; i < Primes.Length; i++)
            {
                var prime = Primes[i];
                if (prime >= number)
                    return prime;
            }
            //expand the array for re-use
            var next = ComputeNextPrime(number);
            if(next>number)
            {
                var temp = new int[primes.Length + 1];
                primes.CopyTo(temp, 0);
                primes = temp;
                primes[primes.Length - 1] = next;
                return next;
            }
            return number;
        }
    }
}