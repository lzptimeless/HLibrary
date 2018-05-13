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
        public HMetadataSegment()
        {
            FileStatus = new HMetadataSegmentFileStatus();
        }

        #region fields
        /// <summary>
        /// 数据段控制码，参见<see cref="HMetadataControlCodes"/>
        /// </summary>
        public abstract byte ControlCode { get; }
        public const string ControlCodePropertyName = "ControlCode";

        /// <summary>
        /// 在文件中的状态
        /// </summary>
        public HMetadataSegmentFileStatus FileStatus { get; private set; }
        public const string FileStatusPropertyName = "FileStatus";
        #endregion

        #region methods
        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="stream">用以保存数据的<see cref="Stream"/></param>
        /// <param name="appendix">附加数据</param>
        /// <param name="reserveLen">保留空间大小，会以<see cref="HMetadataConstant.ControlCodeFlag"/>填充</param>
        public async Task SaveAsync(Stream stream, Stream[] appendixes, int reserveLen)
        {
            ExceptionFactory.CheckArgNull("stream", stream);
            ExceptionFactory.CheckArgRange("reserveLen", reserveLen, 0, int.MaxValue);

            if (appendixes != null)
            {
                for (int i = 0; i < appendixes.Length; i++)
                {
                    if (appendixes[i].Length > int.MaxValue)
                        throw new ArgumentException($"appendixes[{i}] is too big: max={int.MaxValue}, value={appendixes[i].Length}", "appendixes");
                }
            }

            long position = stream.Position;
            int appendixLen = 0;
            foreach (var appendix in appendixes)
            {
                appendixLen = checked(appendixLen + (int)appendix.Length);
            }

            byte[] buffer;
            byte[] fields = GetFields();
            ExceptionFactory.CheckBufferNull("fields", fields, "GetFields return null");
            ExceptionFactory.CheckBufferLengthRange("fields", fields, 0, int.MaxValue, "GetFields return null");
            // 写入控制码
            await stream.WriteByteAsync(HMetadataConstant.ControlCodeFlag);
            await stream.WriteByteAsync(ControlCode);
            // 写入字段长度
            buffer = BitConverter.GetBytes(fields.Length);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            // 写入附加数据长度
            buffer = BitConverter.GetBytes(appendixLen);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            // 写入保留区长度
            buffer = BitConverter.GetBytes(reserveLen);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            // 写入字段
            if (fields.Length > 0)
                await stream.WriteAsync(fields, 0, fields.Length);
            // 填充附加数据
            if (appendixLen > 0)
            {
                foreach (var appendix in appendixes)
                {
                    appendix.Seek(0, SeekOrigin.Begin);
                    await appendix.CopyToAsync(stream);
                }
            }
            // 填充保留区
            if (reserveLen > 0) await stream.FillAsync(HMetadataConstant.ControlCodeFlag, reserveLen);

            // 更新文件状态
            FileStatus.Position = position;
            FileStatus.FieldsLength = fields.Length;
            FileStatus.AppendixLength = appendixLen;
            FileStatus.ReserveLength = reserveLen;
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
            if (intBuffer.Length != await stream.ReadAsync(intBuffer, 0, intBuffer.Length))
                throw new EndOfStreamException("Stream ended when read appendix len");

            appendixLen = BitConverter.ToInt32(intBuffer, 0);
            if (appendixLen < 0)
                throw new InvalidDataException($"Invalid appendix len: expected=[0,{int.MaxValue}] value={appendixLen}");
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

            // 略过附加数据和保留区
            stream.Seek(checked(appendixLen + reserveLen), SeekOrigin.Current);

            // 更新文件状态
            FileStatus.Position = position;
            FileStatus.FieldsLength = fieldsLen;
            FileStatus.AppendixLength = appendixLen;
            FileStatus.ReserveLength = reserveLen;

            SetFields(fields);
        }

        /// <summary>
        /// 获取字段数据的长度
        /// </summary>
        /// <returns></returns>
        public abstract int GetFieldsLength();

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
