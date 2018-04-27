using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class InvalidBufferException : Exception
    {
        public InvalidBufferException(string bufferName, string msg, Exception innerException)
            : base(msg, innerException)
        {
            BufferName = bufferName;
        }

        public string BufferName { get; private set; }

        public override string Message
        {
            get
            {
                string msg = base.Message;
                string append = $"Invalid buffer: {BufferName}";
                if (string.IsNullOrEmpty(msg))
                    return append;
                else
                    return msg + Environment.NewLine + append;
            }
        }
    }
}
