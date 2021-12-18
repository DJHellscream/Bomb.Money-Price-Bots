using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace BombPriceBot
{
    internal static class Extensions
    {
        // Created with thanks to http://stackoverflow.com/questions/16083666/make-big-and-small-numbers-human-readable/16091580#16091580
        static readonly string[] humanReadableSuffixes = { " f", " a", " p", " n", " μ", " m", "", " k", " M", " B", " T", " Quadrillion", " Quintillion", " Sextillion", "Septillion" };
        public static string ToHumanReadable(this double value, int numSignificantDigits)
        {

            // Deal with special values
            if (double.IsInfinity(value) || double.IsNaN(value) || value == 0 || numSignificantDigits <= 0)
                return value.ToString();

            // We deal only with positive values in the code below
            var isNegative = Sign(value) < 0;
            value = Abs(value);

            // Calculate the exponent as a multiple of 3, ie -6, -3, 0, 3, 6, etc
            var exponent = (int)Floor(Log10(value) / 3) * 3;

            // Find the correct suffix for the exponent, or fall back to scientific notation
            var indexOfSuffix = exponent / 3 + 6;
            var suffix = indexOfSuffix >= 0 && indexOfSuffix < humanReadableSuffixes.Length
                ? humanReadableSuffixes[indexOfSuffix]
                : "·10^" + exponent;

            // Scale the value to the exponent, then format it to the correct number of significant digits and add the suffix
            value = value * Pow(10, -exponent);
            var numIntegerDigits = (int)Floor(Log(value, 10)) + 1;
            var numFractionalDigits = Min(numSignificantDigits - numIntegerDigits, 15);
            var format = $"{new string('0', numIntegerDigits)}.{new string('0', numFractionalDigits)}";
            var result = value.ToString(format) + suffix;

            // Handle negatives
            if (isNegative)
                result = "-" + result;

            return result;
        }
    }
}