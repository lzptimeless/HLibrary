using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBook
{
    public class HBookPartStream : Stream
    {
        public HBookPartStream(HBookStream parentStream, long partPosition, long partLength)
        {
            if (parentStream == null)
                throw new ArgumentNullException("parentStream");

            if (partPosition < 0 || partPosition >= parentStream.Length)
                throw new ArgumentOutOfRangeException("partPosition", $"parentStream length:{parentStream.Length}, partPosition:{partPosition}");

            if (partLength <= 0 || partLength > parentStream.Length - partPosition)
                throw new ArgumentOutOfRangeException("partLength", $"parentStream length:{parentStream.Length}, partPosition:{partPosition}, partLength:{partLength}");

            ParentStream = parentStream;
            PartPosition = partPosition;
            PartLength = partLength;
        }

        public HBookStream ParentStream { get; private set; }
        public long PartPosition { get; private set; }
        public long PartLength { get; private set; }
        public bool IsDisposed { get; private set; }

        public override bool CanRead
        {
            get
            {
                return ParentStream != null && ParentStream.CanRead && !IsDisposed;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return ParentStream != null && ParentStream.CanSeek && !IsDisposed;
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
                if (ParentStream == null || IsDisposed)
                    return 0;

                return ParentStream.Position - PartPosition;
            }

            set
            {
                if (ParentStream == null)
                    ThrowParentStreamIsNull();

                if (IsDisposed)
                    ThrowIsDisposed();

                if (value < 0 || value >= PartLength)
                    throw new ArgumentOutOfRangeException("value", $"PartLength:{PartLength}, value:{value}");

                ParentStream.Position = value + PartPosition;
            }
        }

        public override void Flush()
        {
            if (ParentStream == null)
                ThrowParentStreamIsNull();

            if (IsDisposed)
                ThrowIsDisposed();

            ParentStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (ParentStream == null)
                ThrowParentStreamIsNull();

            if (IsDisposed)
                ThrowIsDisposed();

            // 防止读取数据超过尾部
            if (count > PartLength - Position)
                count = (int)(PartLength - Position);

            return ParentStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (ParentStream == null)
                ThrowParentStreamIsNull();

            if (IsDisposed)
                ThrowIsDisposed();

            if (origin == SeekOrigin.Current)
            {
                if (Position + offset < 0 || Position + offset >= PartLength)
                    throw new ArgumentOutOfRangeException("offset", $"Position:{Position}, PartLength:{PartLength}, offset:{offset}, origin:Current");

                return ParentStream.Seek(offset, SeekOrigin.Current);
            }
            else if (origin == SeekOrigin.Begin)
            {
                if (offset < 0 || offset >= PartLength)
                    throw new ArgumentOutOfRangeException("offset", $"PartLength:{PartLength}, offset:{offset}, origin:Begin");

                return ParentStream.Seek(offset + PartPosition, SeekOrigin.Begin);
            }
            else
            {
                if (offset < 0 || offset >= PartLength)
                    throw new ArgumentOutOfRangeException("offset", $"PartLength:{PartLength}, offset:{offset}, origin:End");

                return ParentStream.Seek(PartPosition + PartLength - 1 - offset, SeekOrigin.Begin);
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
            IsDisposed = true;
        }

        private static void ThrowParentStreamIsNull()
        {
            throw new ApplicationException("ParentStream is null");
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
