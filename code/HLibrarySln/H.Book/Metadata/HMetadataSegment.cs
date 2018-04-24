using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public abstract class HMetadataSegment
    {
        /// <summary>
        /// 空数据,长度1024，以<see cref="HMetadataConstant.ControlCodeFlag"/>填充,用以加快
        /// 填充空白数据的效率
        /// </summary>
        protected static readonly byte[] EmptyData;

        static HMetadataSegment()
        {
            // 初始化EmptyData
            byte[] emptyData = new byte[1024];
            for (int i = 0; i < emptyData.Length; i++)
            {
                emptyData[i] = HMetadataConstant.ControlCodeFlag;
            }
            EmptyData = emptyData;
        }

        /// <summary>
        /// 数据段控制码，参见<see cref="HMetadataControlCodes"/>
        /// </summary>
        public abstract byte ControlCode { get; }
        /// <summary>
        /// 数据段在整个<see cref="HBook"/>中的起始位置
        /// </summary>
        public long Position { get; set; }
        /// <summary>
        /// 数据大小 4B 包括除了控制码，本字段本身，保留区大小字段，保留区的所有数据的长度，用以快速定位下一个数据段
        /// </summary>
        public int DataLength { get; set; }
        /// <summary>
        /// 保留区大小 4B 保留区长度，用以快速定位下一个数据段
        /// </summary>
        public int ReserveLength { get; set; }

        /// <summary>
        /// 获取保存这个数据段所需的大小，不包括保留区，会调用<see cref="GetDataLength"/>
        /// </summary>
        public int GetDesiredLength()
        {
            int dataLen = GetDataLength();
            return GetDesiredLengthInner(dataLen);
        }

        /// <summary>
        /// 保存数据，会调用<see cref="GetDataLength"/>，<see cref="GetData"/>
        /// </summary>
        /// <param name="stream">用以保存数据的<see cref="Stream"/></param>
        /// <param name="space">stream中可用以保存数据的空间大小，用剩的空间会以<see cref="HMetadataConstant.ControlCodeFlag"/>填充</param>
        public void Save(Stream stream, int space)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            // 总数据长度：控制码长度+数据长度的长度+保留区长度的长度+数据长度
            long position = stream.Position;
            int dataLen = GetDataLength();
            long desiredLen = GetDesiredLengthInner(dataLen);
            int reserveLen = space - (int)desiredLen;
            if (desiredLen > space)
                throw new ArgumentOutOfRangeException("space", $"The space is not enough to save data: desiredLen={desiredLen}, space={space}");

            byte[] buffer;
            byte[] data = GetData();
            if (data == null)
                throw new ApplicationException("GetBytes return null");

            if (dataLen != data.LongLength)
                throw new ApplicationException($"GetBytes returned data length({data.LongLength}) not equal to GetDataLength({dataLen})");

            // 写入控制码
            stream.WriteByte(HMetadataConstant.ControlCodeFlag);
            stream.WriteByte(ControlCode);
            // 写入数据长度
            buffer = BitConverter.GetBytes(dataLen);
            stream.Write(buffer, 0, buffer.Length);
            // 写入保留区长度
            buffer = BitConverter.GetBytes(reserveLen);
            stream.Write(buffer, 0, buffer.Length);
            // 写入数据
            stream.Write(data, 0, data.Length);
            // 填充保留区
            if (reserveLen > 0)
                FillEmptyData(stream, reserveLen);

            Position = position;
            DataLength = dataLen;
            ReserveLength = reserveLen;
        }

        /// <summary>
        /// 从stream中的读取数据，会调用<see cref="LoadData(byte[])"/>
        /// </summary>
        /// <param name="stream">数据源</param>
        public void Load(Stream stream)
        {
            const int byteResultEnd = -1;
            int byteResult;
            // 数据段起始位置
            long position = stream.Position;
            // 数据长度
            int dataLen;
            // 保留区长度
            int reserveLen;
            // 数据缓存
            byte[] data = null;
            // 验证控制码
            byteResult = stream.ReadByte();
            if (byteResult == byteResultEnd)
                throw new EndOfStreamException("Stream ended when read control code flag");

            if (HMetadataConstant.ControlCodeFlag != byteResult)
                throw new InvalidDataException($"Invalid control code flag: expected={HMetadataConstant.ControlCodeFlag}, value={byteResult}");

            byteResult = stream.ReadByte();
            if (byteResult == byteResultEnd)
                throw new EndOfStreamException("Stream ended when read control code");

            if (ControlCode != byteResult)
                throw new InvalidDataException($"Invalid control code: expected={ControlCode}, value={byteResult}");

            // 读取数据长度
            byte[] dataLenBuffer = new byte[4];
            if (dataLenBuffer.Length != stream.Read(dataLenBuffer, 0, dataLenBuffer.Length))
                throw new EndOfStreamException("Stream ended when read data len");

            dataLen = BitConverter.ToInt32(dataLenBuffer, 0);
            if (dataLen < 0)
                throw new InvalidDataException($"Invalid data len: expected=[0,{int.MaxValue}] value={dataLen}");

            // 读取保留区长度
            byte[] reserveLenBuffer = new byte[4];
            if (reserveLenBuffer.Length != stream.Read(reserveLenBuffer, 0, reserveLenBuffer.Length))
                throw new EndOfStreamException("Stream ended when read reserve len");

            reserveLen = BitConverter.ToInt32(reserveLenBuffer, 0);
            if (reserveLen < 0)
                throw new InvalidDataException($"Invalid reserve len: expected=[0,{int.MaxValue}] value={reserveLen}");

            // 读取数据
            if (dataLen > 0)
            {
                data = new byte[dataLen];
                if (dataLen != stream.Read(data, 0, data.Length))
                    throw new EndOfStreamException("Stream ended when read data");
            }

            Position = position;
            DataLength = dataLen;
            ReserveLength = reserveLen;
            LoadData(data);
        }

        /// <summary>
        /// 用<see cref="HMetadataConstant.ControlCodeFlag"/>填充
        /// </summary>
        /// <param name="stream">填充的对象</param>
        /// <param name="len">填充的长度</param>
        protected void FillEmptyData(Stream stream, long len)
        {
            while (len > 0)
            {
                int writeLen = (int)Math.Min(EmptyData.Length, len);
                stream.Write(EmptyData, 0, writeLen);
                len = len - writeLen;
            }
        }

        /// <summary>
        /// 获取保存这个数据段所需的大小，不包括保留区
        /// </summary>
        /// <param name="dataLen">数据长度</param>
        /// <returns></returns>
        protected int GetDesiredLengthInner(int dataLen)
        {
            //控制码长度+数据长度的长度+保留区长度的长度+数据长度
            int headerLen = 2 + 4 + 4;
            if (int.MaxValue - headerLen < dataLen)
                throw new ArgumentOutOfRangeException($"dataLen is too big: expected=[0,{int.MaxValue - headerLen}], value={dataLen}");

            int totalLen = headerLen + dataLen;
            return totalLen;
        }

        /// <summary>
        /// 获取数据，不包括控制码+数据长度+保留区长度+保留区
        /// </summary>
        /// <returns>数据</returns>
        protected abstract byte[] GetData();

        /// <summary>
        /// 获取数据长度，不包括控制码的长度+数据长度的长度+保留区长度的长度+保留区的长度
        /// </summary>
        /// <returns>数据长度</returns>
        protected abstract int GetDataLength();

        /// <summary>
        /// 加载数据，不包括控制码+数据长度+保留区长度+保留区
        /// </summary>
        /// <param name="data">数据</param>
        protected abstract void LoadData(byte[] data);
    }
}
