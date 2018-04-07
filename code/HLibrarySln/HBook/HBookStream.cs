using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBook
{
    public class HBookStream : Stream
    {
        #region fields
        /// <summary>
        /// 整个<see cref="HBookStream"/>的锁
        /// </summary>
        private static object _lockObj = new object();
        /// <summary>
        /// 在创建<see cref="HBookPartStream"/>时会同时生成一个操作码，并将这个操作码传入创建
        /// 的<see cref="HBookPartStream"/>，之后所有对本对象的读取和写入操作都必须通过这个操
        /// 作码（对外部来说就必须通过唯一的<see cref="HBookPartStream"/>对象访问），否则就会
        /// 抛出异常，这种限制可以强制外部操作代码必须简洁
        /// </summary>
        private long _operateCode;
        #endregion

        public override bool CanRead
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanSeek
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanWrite
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public long SetPosition(long newPosition, long operateCode)
        {
            _operateCode = operateCode;
            return 0;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
