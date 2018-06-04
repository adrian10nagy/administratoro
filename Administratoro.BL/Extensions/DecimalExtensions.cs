

namespace Administratoro.BL.Extensions
{
    using System;

    public static class DecimalExtensions
    {
        public static decimal RoundUp(double input, int places)
        {
            double multiplier = Math.Pow(10, Convert.ToDouble(places));
            decimal result = (decimal)(Math.Ceiling(input * multiplier) / multiplier);

            return result;
        }

        public static decimal? GetNullableDecimal(string theValue)
        {
            decimal? theValueToUpdate = null;
            decimal valueToUpdate;

            if (decimal.TryParse(theValue, out valueToUpdate))
            {
                theValueToUpdate = valueToUpdate;
            }

            return theValueToUpdate;
        }
    }
}
