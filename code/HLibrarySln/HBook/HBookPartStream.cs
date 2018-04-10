using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HBook
{
    public class HBookPartStream : Stream
    {
        #region fields
        internal Func<bool> ParentCanRead;
        internal Func<bool> ParentCanSeek;
        internal Func<long> ParentGetPosition;
        internal Action<long> ParentSetPosition;
        internal Action ParentFlush;
        internal Func<byte[], int, int, int> ParentRead;
        internal Func<long, SeekOrigin, long> ParentSeek;
        #endregion

        internal HBookPartStream(long partPosition, long partLength)
        {
            PartPosition = partPosition;
            PartLength = partLength;
        }

        #region properties
        public long PartPosition { get; private set; }
        public long PartLength { get; private set; }
        public bool IsDisposed { get; private set; }
        public override bool CanRead
        {
            get
            {
                return ParentCanRead != null && ParentCanRead.Invoke() && !IsDisposed;
            }
        }
        public override bool CanSeek
        {
            get
            {
                return ParentCanSeek != null && ParentCanSeek.Invoke() && !IsDisposed;
            }
        }
        public override bool CanWrite
        {
            get { return false; }
        }
        public override long Length
        {
            get
            {
                return PartLength;
            }
        }
        public override long Position
        {
            get
            {
                if (ParentGetPosition == null || IsDisposed)
                    return 0;

                return ParentGetPosition.Invoke() - PartPosition;
            }

            set
            {
                if (IsDisposed)
                    ThrowIsDisposed();

                if (ParentSetPosition == null)
                    ThrowNotInitialized();

                if (value < 0 || value >= PartLength)
                    throw new ArgumentOutOfRangeException("value", $"PartLength:{PartLength}, value:{value}");

                ParentSetPosition.Invoke(value + PartPosition);
            }
        }
        #endregion

        #region events
        public event EventHandler<HBookPartStreamIsDisposedChangedArgs> IsDisposedChanged;
        private void OnIsDisposedChanged(HBookPartStreamIsDisposedChangedArgs e)
        {
            Volatile.Read(ref IsDisposedChanged)?.Invoke(this, e);
        }
        #endregion

        public override void Flush()
        {
            if (IsDisposed)
                ThrowIsDisposed();

            if (ParentFlush == null)
                ThrowNotInitialized();

            ParentFlush.Invoke();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (IsDisposed)
                ThrowIsDisposed();

            if (ParentRead == null)
                ThrowNotInitialized();

            // 防止读取数据超过尾部
            if (count > PartLength - Position)
                count = (int)(PartLength - Position);

            return ParentRead.Invoke(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (IsDisposed)
                ThrowIsDisposed();

            if (ParentSeek == null)
                ThrowNotInitialized();

            if (origin == SeekOrigin.Current)
            {
                if (Position + offset < 0 || Position + offset >= PartLength)
                    throw new ArgumentOutOfRangeException("offset", $"Position:{Position}, PartLength:{PartLength}, offset:{offset}, origin:Current");

                return ParentSeek.Invoke(offset, SeekOrigin.Current);
            }
            else if (origin == SeekOrigin.Begin)
            {
                if (offset < 0 || offset >= PartLength)
                    throw new ArgumentOutOfRangeException("offset", $"PartLength:{PartLength}, offset:{offset}, origin:Begin");

                return ParentSeek.Invoke(offset + PartPosition, SeekOrigin.Begin);
            }
            else
            {
                if (offset < 0 || offset >= PartLength)
                    throw new ArgumentOutOfRangeException("offset", $"PartLength:{PartLength}, offset:{offset}, origin:End");

                return ParentSeek.Invoke(PartPosition + PartLength - 1 - offset, SeekOrigin.Begin);
            }
        }

        public override void SetLength(long value)
        {
            ThrowNotSupportWrite();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowNotSupportWrite();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            ParentCanRead = null;
            ParentCanSeek = null;
            ParentGetPosition = null;
            ParentSetPosition = null;
            ParentFlush = null;
            ParentRead = null;
            ParentSeek = null;
            IsDisposed = true;
            OnIsDisposedChanged(new HBookPartStreamIsDisposedChangedArgs(IsDisposed));
        }

        private static void ThrowNotInitialized()
        {
            throw new ApplicationException("Not initialized.");
        }

        private static void ThrowNotSupportWrite()
        {
            throw new NotSupportedException("Not support write");
        }

        private static void ThrowIsDisposed()
        {
            throw new ObjectDisposedException("PartFileStream");
        }
    }
}
