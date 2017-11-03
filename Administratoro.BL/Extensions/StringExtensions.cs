namespace Administratoro.BL.Extensions
{
    public static class StringExtensions
    {
        public static int? ToNullableInt(this string input)
        {
            int i;
            if (!string.IsNullOrEmpty(input))
            {
                input = input.Trim();
            }

            if (int.TryParse(input, out i)) return i;
            return null;
        }

        public static decimal? ToNullableDecimal(this string input)
        {
            decimal i;
            if (!string.IsNullOrEmpty(input))
            {
                input = input.Trim();
            }

            if (decimal.TryParse(input, out i)) return i;
            return null;
        }
    }
}
