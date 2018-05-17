using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H.Book
{
    internal class PartReadStream : Stream
    {
        #region fields
        private Stream _parentStream;
        #endregion

        internal PartReadStream(Stream parentStream, long partPosition, long partLength)
        {
            _parentStream = parentStream;
            PartPosition = partPosition;
            PartLength = partLength;
        }

        #region properties
        public long PartPosition { get; private set; }
        public long PartLength { get; private set; }
        public bool IsDisposed { get; private set; }
        public override bool CanTimeout { get { return _parentStream.CanTimeout; } }
        public override int ReadTimeout
        {
            get { return _parentStream.ReadTimeout; }
            set { _parentStream.ReadTimeout = value; }
        }
        public override int WriteTimeout
        {
            get { return _parentStream.WriteTimeout; }
            set { _parentStream.WriteTimeout = value; }
        }
        public override bool CanRead
        {
            get { return _parentStream.CanRead && !IsDisposed; }
        }
        public override bool CanSeek
        {
            get { return _parentStream.CanSeek && !IsDisposed; }
        }
        public override bool CanWrite
        {
            get { return false; }
        }
        public override long Length
        {
            get { return PartLength; }
        }
        public override long Position
        {
            get
            {
                Debug.Assert(_parentStream.Position >= PartPosition && _parentStream.Position <= PartPosition + PartLength);

                return _parentStream.Position - PartPosition;
            }

            set
            {
                if (value < 0 || value > PartLength)
                    throw new ArgumentOutOfRangeException("value", $"PartLength:{PartLength}, value:{value}");

                _parentStream.Position = value + PartPosition;
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
            _parentStream.Flush();
        }

        public override int ReadByte()
        {
            // 防止读取数据超过范围
            if (Position >= PartLength)
                return -1;

            return _parentStream.ReadByte();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // 防止读取数据超过尾部
            if (count > PartLength - Position)
                count = (int)(PartLength - Position);

            if (count == 0) return 0;

            return _parentStream.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // 防止读取数据超过尾部
            if (count > PartLength - Position)
                count = (int)(PartLength - Position);

            if (count == 0) return Task.FromResult<int>(0);

            return _parentStream.ReadAsync(buffer, offset, count);
        }

        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            Debug.Assert(Position == 0, "Position is not 0");
            byte[] buffer = new byte[bufferSize];
            long copyLen = PartLength;
            while (copyLen > 0)
            {
                int len = (int)Math.Min(copyLen, bufferSize);
                int readLen = await _parentStream.ReadAsync(buffer, 0, len);
                if (readLen != len)
                    throw new ApplicationException($"Data length not equal to expected: PartLength={PartLength}, PartPosition={PartPosition}, parent-len={_parentStream.Length}");

                cancellationToken.ThrowIfCancellationRequested();
                await destination.WriteAsync(buffer, 0, len);
                cancellationToken.ThrowIfCancellationRequested();
                copyLen -= len;
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            // 防止读取数据超过尾部
            if (count > PartLength - Position)
                count = (int)(PartLength - Position);

            return _parentStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return _parentStream.EndRead(asyncResult);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Current)
            {
                if (Position + offset < 0 || Position + offset > PartLength)
                    throw new ArgumentOutOfRangeException("offset", $"Position:{Position}, PartLength:{PartLength}, offset:{offset}, origin:Current");

                return _parentStream.Seek(offset, SeekOrigin.Current);
            }
            else if (origin == SeekOrigin.Begin)
            {
                if (offset < 0 || offset > PartLength)
                    throw new ArgumentOutOfRangeException("offset", $"PartLength:{PartLength}, offset:{offset}, origin:Begin");

                return _parentStream.Seek(offset + PartPosition, SeekOrigin.Begin);
            }
            else
            {
                if (offset < 0 || offset > PartLength)
                    throw new ArgumentOutOfRangeException("offset", $"PartLength:{PartLength}, offset:{offset}, origin:End");

                return _parentStream.Seek(PartPosition + PartLength - offset, SeekOrigin.Begin);
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("Not support write");
        }

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException("Not support write");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Not support write");
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Not support write");
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException("Not support write");
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException("Not support write");
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Not support write");
        }

        public override void Close()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            IsDisposed = true;
            OnIsDisposedChanged(new HBookPartStreamIsDisposedChangedArgs(IsDisposed));
        }
    }
}
