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
        private Stream _stream;
        #endregion

        public HBookStream(string path, FileMode mode)
        {
            _stream = new FileStream(path, mode, FileAccess.ReadWrite, FileShare.None);
        }

        #region properties
        #region CurrentPartStream
        private HBookPartStream _currentPartStream;
        /// <summary>
        /// Get or set <see cref="CurrentPartStream"/>
        /// </summary>
        public HBookPartStream CurrentPartStream
        {
            get { return _currentPartStream; }
        }
        #endregion

        public HBookPartStream ReadPart(long partPosition, long partLength)
        {
            HBookPartStream partStream;
            lock (_currentPartStream)
            {
                ThrowCanOnlyAccessCurrentPartStream();

                if (partPosition < 0 || partPosition >= _stream.Length)
                    throw new ArgumentOutOfRangeException("partPosition", $"origin stream length:{_stream.Length}, partPosition:{partPosition}");

                if (partLength <= 0 || partLength > _stream.Length - partPosition)
                    throw new ArgumentOutOfRangeException("partLength", $"origin stream length:{_stream.Length}, partPosition:{partPosition}, partLength:{partLength}");

                partStream = new HBookPartStream(partPosition, partLength);
                partStream.ParentCanRead = PartStreamCanRead;
                partStream.ParentCanSeek = PartStreamCanSeek;
                partStream.ParentGetPosition = PartStreamGetPosition;
                partStream.ParentSetPosition = PartStreamSetPosition;
                partStream.ParentFlush = PartStreamFlush;
                partStream.ParentRead = PartStreamRead;
                partStream.ParentSeek = PartStreamSeek;
                partStream.IsDisposedChanged += PartStream_IsDisposedChanged;
                _currentPartStream = partStream;
            }

            return partStream;
        }

        public override bool CanRead
        {
            get
            {
                return _stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return _stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return _stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return _stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return _stream.Position;
            }

            set
            {
                lock (_lockObj)
                {
                    ThrowCanOnlyAccessCurrentPartStream();
                    _stream.Position = value;
                }
            }
        }
        #endregion

        public override void Flush()
        {
            lock (_lockObj)
            {
                ThrowCanOnlyAccessCurrentPartStream();
                _stream.Flush();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readCount;
            lock (_lockObj)
            {
                ThrowCanOnlyAccessCurrentPartStream();
                readCount = _stream.Read(buffer, offset, count);
            }

            return readCount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPosition;
            lock (_lockObj)
            {
                ThrowCanOnlyAccessCurrentPartStream();
                newPosition = _stream.Seek(offset, origin);
            }

            return newPosition;
        }

        public override void SetLength(long value)
        {
            lock (_lockObj)
            {
                ThrowCanOnlyAccessCurrentPartStream();
                _stream.SetLength(value);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_lockObj)
            {
                ThrowCanOnlyAccessCurrentPartStream();
                _stream.Write(buffer, offset, count);
            }
        }

        private void PartStream_IsDisposedChanged(object sender, HBookPartStreamIsDisposedChangedArgs e)
        {
            lock (_lockObj)
            {
                if (_currentPartStream != null)
                {
                    _currentPartStream.IsDisposedChanged -= PartStream_IsDisposedChanged;
                    _currentPartStream = null;
                }
            }
        }

        private bool PartStreamCanRead()
        {
            return _stream.CanRead;
        }

        private bool PartStreamCanSeek()
        {
            return _stream.CanSeek;
        }

        private long PartStreamGetPosition()
        {
            return _stream.Position;
        }

        private void PartStreamSetPosition(long position)
        {
            lock (_lockObj)
            {
                ThrowCurrentPartStreamIsNullOrDisposed();
                _stream.Position = position;
            }
        }

        private void PartStreamFlush()
        {
            lock (_lockObj)
            {
                ThrowCurrentPartStreamIsNullOrDisposed();
                _stream.Flush();
            }
        }

        private int PartStreamRead(byte[] buffer, int offset, int count)
        {
            int readCount;
            lock (_lockObj)
            {
                ThrowCurrentPartStreamIsNullOrDisposed();
                readCount = _stream.Read(buffer, offset, count);
            }

            return readCount;
        }

        private long PartStreamSeek(long offset, SeekOrigin origin)
        {
            long newPosition;
            lock (_lockObj)
            {
                ThrowCurrentPartStreamIsNullOrDisposed();
                newPosition = _stream.Seek(offset, origin);
            }

            return newPosition;
        }

        private void ThrowCurrentPartStreamIsNullOrDisposed()
        {
            if (_currentPartStream == null || _currentPartStream.IsDisposed)
                throw new ApplicationException("CurrentPartStream is null or disposed");
        }

        private void ThrowCanOnlyAccessCurrentPartStream()
        {
            if (_currentPartStream != null && !_currentPartStream.IsDisposed)
                throw new ApplicationException("Can only access CurrentPartStream");
        }
    }
}
