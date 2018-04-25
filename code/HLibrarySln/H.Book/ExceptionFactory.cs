using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    internal static class ExceptionFactory
    {
        public static void CheckArgNull(string argName, object arg)
        {
            if (arg == null)
                throw new ArgumentNullException(argName);
        }

        public static void CheckArgRange(string argName, int arg, int min, int max)
        {
            if (arg < min || arg > max)
                throw new ArgumentOutOfRangeException(argName, $"Invalid {argName}: expected=[{min},{max}], value={arg}");
        }

        public static void CheckPropertyEmpty(string propertyName, Guid value)
        {
            if (value == Guid.Empty)
                throw new ApplicationException($"Invalid {propertyName}: can not be empty");
        }

        public static void CheckPropertyRange(string propertyName, int value, int min, int max)
        {
            if (value < min || value > max)
                throw new ApplicationException($"Invalid {propertyName}: expected=[{min},{max}], value={value}");
        }

        public static void CheckPropertyCountRange<T>(string propertyName, IList<T> property, int min, int max)
        {
            if (property == null)
                return;

            if (property.Count < min || property.Count > max)
                throw new ApplicationException($"{propertyName} count valid: expected=[{min},{max}], value={property.Count}");
        }

        public static void CheckBufferLength(string bufferName, byte[] bytes, int len)
        {
            if (bytes.Length != len)
                throw new ApplicationException($"{bufferName} length error: expected={len}, value={bytes.Length}");
        }

        public static void CheckBufferLengthRange(string bufferName, byte[] buffer, int min, int max)
        {
            if (buffer.Length < min || buffer.Length > max)
                throw new ApplicationException($"{bufferName} length error: expected=[{min},{max}], value={buffer.Length}");
        }
    }
}
