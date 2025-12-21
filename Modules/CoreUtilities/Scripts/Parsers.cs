using System.Globalization;

namespace HitTrax.CoreUtilities
{
    /// <summary>
    /// This mainly exists because in our old suite logic we have a lot of values that call SQL.intParase SQL.float parse etc.
    /// This exists so we can use those functions without breaking the code during refactor
    /// </summary>
    public static class Parsers
    {
        public static int IntParse(this string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                {
                    return 0;
                }
                return int.Parse(str, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        public static long LongParse(this string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                {
                    return 0;
                }
                return long.Parse(str, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        public static float FloatParse(this string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                {
                    return 0;
                }
                return float.Parse(str, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

    }
}
