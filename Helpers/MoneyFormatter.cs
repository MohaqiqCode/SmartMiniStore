using System.Globalization;

namespace SmartMiniStore.Helpers
{
    /// <summary>
    /// Formats money values in Afghan Afghani (AFN).
    /// Example: 4250 -> "4,250 AFN"
    /// </summary>
    public static class MoneyFormatter
    {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        public static string Format(decimal value)
        {
            return value.ToString("N0", Culture) + " AFN";
        }

        public static string Format(decimal value, int decimals)
        {
            return value.ToString("N" + decimals, Culture) + " AFN";
        }
    }
}