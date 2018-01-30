using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Administratoro.BL.Extensions
{
    public static class IntExtensions
    {
        public static int? GetYear(this string input)
        {
            int? year = null;
            int yearVal;
            if (!string.IsNullOrEmpty(input) && int.TryParse(input, out yearVal))
            {
                if (yearVal > 2010 && yearVal < 2100)
                {
                    year = yearVal;
                }
            }

            return year;
        }

        public static int? GetMonth(this string input)
        {
            int? month = null;
            int monthVal;
            if (!string.IsNullOrEmpty(input) && int.TryParse(input, out monthVal))
            {
                if (monthVal > 0 && monthVal < 13)
                {
                    month = monthVal;
                }
            }

            return month;
        }

        public static int? GetInt(this string input)
        {
            int? value = null;
            int theVal;
            if (!string.IsNullOrEmpty(input) && int.TryParse(input, out theVal))
            {
                value = theVal;
            }

            return value;
        }
    }
}
