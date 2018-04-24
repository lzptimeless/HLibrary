using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    internal static class ExceptionFactory
    {
        public static void CheckNull(object arg, string argName)
        {
            if (arg == null)
                throw new ArgumentNullException(argName);
        }

        public static void CheckArgRange(int arg, int min, int max, string argName)
        {
            if (arg < min || arg > max)
                throw new ArgumentOutOfRangeException(argName, $"Invalid {argName}: expected=[{min},{max}], value={arg}");
        }
    }
}
