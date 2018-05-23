using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRTools
{
    public class LockWaitLimitException : Exception
    {
        public LockWaitLimitException(int limit, string msg)
            : base(msg)
        {
            Limit = limit;
        }

        public int Limit { get; private set; }

        public override string Message
        {
            get
            {
                string msg = base.Message;
                string append = $"Wait count out of limit: limit={Limit}";
                if (string.IsNullOrEmpty(msg))
                    return append;
                else
                    return msg + Environment.NewLine + append;
            }
        }
    }
}
