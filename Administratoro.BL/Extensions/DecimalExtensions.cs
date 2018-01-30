using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Administratoro.BL.Extensions
{
    public static class DecimalExtensions
    {
        public static decimal RoundUp(double input, int places)
        {
            double multiplier = Math.Pow(10, Convert.ToDouble(places));
            decimal result = (decimal)(Math.Ceiling(input * multiplier) / multiplier);

            return result;
        }
    }
}
