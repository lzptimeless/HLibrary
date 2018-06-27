using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H.BookLibrary
{
    public static class Output
    {
        #region events
        #region Write
        /// <summary>
        /// 写日志事件
        /// </summary>
        public static event EventHandler<WriteEventArgs> Write;

        private static void OnWrite(WriteEventArgs e)
        {
            Volatile.Read(ref Write)?.Invoke(null, e);
        }
        #endregion
        #endregion

        public static void Print(string msg)
        {
            OnWrite(new WriteEventArgs(msg));
            Console.WriteLine(msg);
        }
    }

    public class WriteEventArgs : EventArgs
    {
        public WriteEventArgs(string msg)
        {
            Message = msg;
        }

        public string Message { get; private set; }
    }
}
