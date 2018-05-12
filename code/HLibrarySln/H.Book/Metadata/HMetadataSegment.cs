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
        #region fields
        /// <summary>
        /// 数据段控制码，参见<see cref="HMetadataControlCodes"/>
        /// </summary>
        public abstract byte ControlCode { get; }
        public const string ControlCodePropertyName = "ControlCode";

        /// <summary>
        /// 初始化时保留区长度
        /// </summary>
        protected abstract int InitReserveLength { get; }
        protected const string InitReserveLengthPropertyName = "InitReserveLength";

        /// <summary>
        /// 在文件中的状态
        /// </summary>
        private HMetadataSegmentFileStatus _fileStatus;
        public HMetadataSegmentFileStatus FileStatus { get { return _fileStatus; } }
        public const string FileStatusPropertyName = "FileStatus";
        #endregion

        #region methods
        /// <summary>
        /// 获取保存这个数据段所需的大小，不包括保留区，会调用<see cref="GetFieldsLength"/>和<see cref="GetAppendixLength"/>
        /// </summary>
        public int GetDesiredLength()
        {
            int dataLen = GetFieldsLength();
            int appenLen = GetAppendixLength();
            return GetDesiredLengthInner(dataLen, appenLen);
        }

        /// <summary>
        /// 在文件中创建数据段
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <returns></returns>
        public Task CreateAsync(Stream stream)
        {
            ExceptionFactory.CheckPropertyRange(InitReserveLengthPropertyName, InitReserveLength, 0, int.MaxValue);
            int space = checked(GetDesiredLength() + InitReserveLength);
            return SaveAsync(stream, space);
        }

        /// <summary>
        /// 保存数据，会调用<see cref="GetFieldsLength"/>，<see cref="GetAppendixLength"/>，<see cref="GetFields"/>
        /// </summary>
        /// <param name="stream">用以保存数据的<see cref="Stream"/></param>
        /// <param name="space">stream中可用以保存数据的空间大小，用剩的空间会以<see cref="HMetadataConstant.ControlCodeFlag"/>填充</param>
        public async Task SaveAsync(Stream stream, int space)
        {
            ExceptionFactory.CheckArgNull("stream", stream);

            long position = stream.Position;
            int fieldsLen = GetFieldsLength();
            int appendixLen = GetAppendixLength();
            int desiredLen = GetDesiredLengthInner(fieldsLen, appendixLen);
            int reserveLen = space - (int)desiredLen;
            ExceptionFactory.CheckArgRange("space", space, desiredLen, int.MaxValue, "The space is not enough to save data");

            byte[] buffer;
            byte[] fields = GetFields();
            ExceptionFactory.CheckBufferNull("fields", fields, "GetFields return null");
            ExceptionFactory.CheckBufferLength("fields", fields, fieldsLen, "GetFields returned data");
            // 写入控制码
            await stream.WriteByteAsync(HMetadataConstant.ControlCodeFlag);
            await stream.WriteByteAsync(ControlCode);
            // 写入字段长度
            buffer = BitConverter.GetBytes(fieldsLen);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            // 写入附加数据长度
            buffer = BitConverter.GetBytes(appendixLen);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            // 写入保留区长度
            buffer = BitConverter.GetBytes(reserveLen);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            // 写入字段
            await stream.WriteAsync(fields, 0, fields.Length);
            // 填充附加数据
            if (appendixLen > 0) await stream.FillAsync(0, appendixLen);
            // 填充保留区
            if (reserveLen > 0) await stream.FillAsync(HMetadataConstant.ControlCodeFlag, reserveLen);

            // 更新文件状态
            _fileStatus.Position = position;
            _fileStatus.FieldsLength = fieldsLen;
            _fileStatus.AppendixLength = appendixLen;
            _fileStatus.ReserveLength = reserveLen;
        }

        /// <summary>
        /// 从stream中的读取数据，会调用<see cref="SetFields(byte[])"/>
        /// </summary>
        /// <param name="stream">数据源</param>
        /// <param name="hasControlCode">数据源包涵控制码，需要检测控制码</param>
        public async Task LoadAsync(Stream stream, bool hasControlCode)
        {
            ExceptionFactory.CheckArgNull("stream", stream);

            const int byteResultEnd = -1;
            int byteResult;
            // 数据段起始位置
            long position = stream.Position;
            // 字段数据长度
            int fieldsLen;
            // 附加数据长度
            int appendixLen;
            // 保留区长度
            int reserveLen;
            // 字段缓存
            byte[] fields = null;
            // 验证控制码
            if (hasControlCode)
            {
                byteResult = await stream.ReadByteAsync();
                if (byteResult == byteResultEnd)
                    throw new EndOfStreamException("Stream ended when read control code flag");

                if (HMetadataConstant.ControlCodeFlag != byteResult)
                    throw new InvalidDataException($"Invalid control code flag: expected={HMetadataConstant.ControlCodeFlag}, value={byteResult}");

                byteResult = await stream.ReadByteAsync();
                if (byteResult == byteResultEnd)
                    throw new EndOfStreamException("Stream ended when read control code");

                if (ControlCode != byteResult)
                    throw new InvalidDataException($"Invalid control code: expected={ControlCode}, value={byteResult}");
            }
            byte[] intBuffer = new byte[4];
            // 读取字段数据长度
            if (intBuffer.Length != await stream.ReadAsync(intBuffer, 0, intBuffer.Length))
                throw new EndOfStreamException("Stream ended when read fields len");

            fieldsLen = BitConverter.ToInt32(intBuffer, 0);
            if (fieldsLen < 0)
                throw new InvalidDataException($"Invalid fields len: expected=[0,{int.MaxValue}] value={fieldsLen}");

            // 读取附加数据长度
            if (intBuffer.Length!=await stream.ReadAsync(intBuffer, 0, intBuffer.Length))
                throw new EndOfStreamException("Stream ended when read appendix len");

            appendixLen = BitConverter.ToInt32(intBuffer, 0);
            if (fieldsLen < 0)
                throw new InvalidDataException($"Invalid appendix len: expected=[0,{int.MaxValue}] value={fieldsLen}");
            // 读取保留区长度
            if (intBuffer.Length != await stream.ReadAsync(intBuffer, 0, intBuffer.Length))
                throw new EndOfStreamException("Stream ended when read reserve len");

            reserveLen = BitConverter.ToInt32(intBuffer, 0);
            if (reserveLen < 0)
                throw new InvalidDataException($"Invalid reserve len: expected=[0,{int.MaxValue}] value={reserveLen}");

            // 读取数据
            if (fieldsLen > 0)
            {
                fields = new byte[fieldsLen];
                if (fieldsLen != await stream.ReadAsync(fields, 0, fields.Length))
                    throw new EndOfStreamException("Stream ended when read fields");
            }
            // 更新文件状态
            _fileStatus.Position = position;
            _fileStatus.FieldsLength = fieldsLen;
            _fileStatus.AppendixLength = appendixLen;
            _fileStatus.ReserveLength = reserveLen;

            SetFields(fields);
        }

        /// <summary>
        /// 获取保存这个数据段所需的大小，不包括保留区
        /// </summary>
        /// <param name="fieldsLen">字段数据长度</param>
        /// <param name="appendixLen">附加数据长度</param>
        /// <returns></returns>
        protected int GetDesiredLengthInner(int fieldsLen, int appendixLen)
        {
            return checked(_fileStatus.GetHeaderLength() + fieldsLen + appendixLen);
        }

        /// <summary>
        /// 获取字段数据长度
        /// </summary>
        /// <returns>字段数据长度</returns>
        protected abstract int GetFieldsLength();

        /// <summary>
        /// 获取附加数据长度
        /// </summary>
        /// <returns>附加数据长度</returns>
        protected abstract int GetAppendixLength();

        /// <summary>
        /// 获取字段数据
        /// </summary>
        /// <returns>字段数据</returns>
        protected abstract byte[] GetFields();

        /// <summary>
        /// 设置字段数据
        /// </summary>
        /// <param name="buffer">字段数据</param>
        protected abstract void SetFields(byte[] buffer);
        #endregion
    }
}
