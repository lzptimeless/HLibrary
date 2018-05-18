using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    internal static class ExceptionFactory
    {
        public static void CheckArgNull(string argName, object arg, string msg = null)
        {
            if (arg == null)
                throw new ArgumentNullException(argName, msg);
        }

        public static void CheckArgRange(string argName, int arg, long min, long max, string msg = null)
        {
            if (arg < min || arg > max)
            {
                string append = $"Range error: expected=[{min},{max}], value={arg}";
                if (string.IsNullOrEmpty(msg))
                    msg = append;
                else
                    msg = msg + Environment.NewLine + append;

                throw new ArgumentOutOfRangeException(argName, msg);
            }
        }

        public static void CheckArgCountRange<T>(string argName, IList<T> arg, long min, long max, string msg = null)
        {
            if (arg == null)
                return;

            if (arg.Count < min || arg.Count > max)
            {
                string append = $"Count error: expected=[{min},{max}], value={arg.Count}";
                if (string.IsNullOrEmpty(msg))
                    msg = append;
                else
                    msg = msg + Environment.NewLine + append;

                throw new ArgumentOutOfRangeException(argName, msg);
            }
        }

        public static void CheckArgLengthRange(string argName, byte[] arg, long min, long max, string msg = null)
        {
            if (arg.Length < min || arg.Length > max)
            {
                string append = $"Length error: expected=[{min},{max}], value={arg.Length}";
                if (string.IsNullOrEmpty(msg))
                    msg = append;
                else
                    msg = msg + Environment.NewLine + append;

                throw new ArgumentOutOfRangeException(argName, msg);
            }
        }

        public static void CheckPropertyEmpty(string propertyName, Guid value, string msg = null)
        {
            if (value == Guid.Empty)
            {
                string append = "Can not be empty";
                if (string.IsNullOrEmpty(msg))
                    msg = append;
                else
                    msg = msg + Environment.NewLine + append;

                throw new InvalidPropertyException(propertyName, msg, null);
            }
        }

        public static void CheckPropertyRange(string propertyName, int value, long min, long max, string msg = null)
        {
            if (value < min || value > max)
            {
                string append = $"Expected=[{min},{max}], value={value}";
                if (string.IsNullOrEmpty(msg))
                    msg = append;
                else
                    msg = msg + Environment.NewLine + append;

                throw new InvalidPropertyException(propertyName, msg, null);
            }
        }

        public static void CheckPropertyCountRange<T>(string propertyName, IList<T> property, long min, long max, string msg = null)
        {
            if (property == null)
                return;

            if (property.Count < min || property.Count > max)
            {
                string append = $"Count error: expected=[{min},{max}], value={property.Count}";
                if (string.IsNullOrEmpty(msg))
                    msg = append;
                else
                    msg = msg + Environment.NewLine + append;

                throw new InvalidPropertyException(propertyName, msg, null);
            }
        }

        public static void CheckBufferNull(string bufferName, byte[] buffer, string msg = null)
        {
            if (buffer == null)
            {
                string append = $"Null error";
                if (string.IsNullOrEmpty(msg))
                    msg = append;
                else
                    msg = msg + Environment.NewLine + append;

                throw new InvalidBufferException(bufferName, msg, null);
            }
        }

        public static void CheckBufferLength(string bufferName, byte[] buffer, long len, string msg = null)
        {
            if (buffer.LongLength != len)
            {
                string append = $"Length error: expected={len}, value={buffer.Length}";
                if (string.IsNullOrEmpty(msg))
                    msg = append;
                else
                    msg = msg + Environment.NewLine + append;

                throw new InvalidBufferException(bufferName, msg, null);
            }
        }

        public static void CheckBufferLengthRange(string bufferName, byte[] buffer, long min, long max, string msg = null)
        {
            if (buffer.LongLength < min || buffer.LongLength > max)
            {
                string append = $"Length error: expected=[{min},{max}], value={buffer.Length}";
                if (string.IsNullOrEmpty(msg))
                    msg = append;
                else
                    msg = msg + Environment.NewLine + append;

                throw new InvalidBufferException(bufferName, msg, null);
            }
        }

        public static Exception CreateWritePropertyException(string propertyName, string msg, Exception innerException)
        {
            return new WritePropertyException(propertyName, msg, innerException);
        }

        public static Exception CreateReadPropertyException(string propertyName, string msg, Exception innerException)
        {
            return new ReadPropertyException(propertyName, msg, innerException);
        }

        public static Exception CreatePageNotFoundEx(Guid id)
        {
            return new PageNotFoundException($"Page not exist: id={id}");
        }
    }
}
