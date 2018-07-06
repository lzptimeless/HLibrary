using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public abstract class HMetadataSegment : ICloneable
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
        /// 元素数据固定长度，<see cref="HMetadataConstant.VariableLength"/>表示可变长度
        /// </summary>
        public abstract int FixedLength { get; }
        public const string FixedLengthPropertyName = "FixedLength";

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
        /// <param name="reserveLen">保留空间大小，会以<see cref="HMetadataConstant.CCFlag"/>填充，如果<see cref="FixedLength"/>有有效值则忽略这个参数</param>
        public async Task SaveAsync(Stream stream, object[] appendixes, int reserveLen)
        {
            ExceptionFactory.CheckArgNull("stream", stream);
            ExceptionFactory.CheckArgRange("reserveLen", reserveLen, 0, int.MaxValue);
            long position = stream.Position;
            int[] appendixLens = new int[appendixes != null ? appendixes.Length : 0];
            for (int i = 0; i < appendixLens.Length; i++)
            {
                long appendixLen;
                if (appendixes[i] is Stream)
                    appendixLen = (appendixes[i] as Stream).Length;
                else if (appendixes[i] is Array)
                    appendixLen = (appendixes[i] as byte[]).LongLength;
                else
                    throw new ArgumentException($"appendixes[{i}] only support Stream or byte[] type", "appendixes");

                if (appendixLen <= 0 || appendixLen > int.MaxValue)
                    throw new ArgumentException($"appendixes[{i}] length error: expected=[1,{int.MaxValue}], value={appendixLen}", "appendixes");

                appendixLens[i] = (int)appendixLen;
            }

            byte[] buffer;
            byte[] fields = GetFields();
            ExceptionFactory.CheckBufferNull("fields", fields, "GetFields return null");
            ExceptionFactory.CheckBufferLengthRange("fields", fields, 0, int.MaxValue, "GetFields return null");

            // 如果总长度固定则验证控件是否足够
            int desiredSpace = 0;
            if (FixedLength != HMetadataConstant.VariableLength)
            {
                if (FixedLength <= 0)
                    throw new InvalidPropertyException(FixedLengthPropertyName, $"value not valid:value={FixedLength}, range={HMetadataConstant.VariableLength}|[1,{int.MaxValue}]", null);

                desiredSpace = HMetadataSegmentFileStatus.CalculateSpace(fields.Length, appendixLens, 0);
                if (FixedLength < desiredSpace)
                    throw new ArgumentException($"Space not enough: {FixedLengthPropertyName}={FixedLength}, desiredSpace={desiredSpace}, fieldsLen={fields.Length}, appendixLens={string.Join(",", appendixLens)}");
            }

            // 写入控制码
            await stream.WriteByteAsync(HMetadataConstant.CCFlag);
            await stream.WriteByteAsync(ControlCode);
            // 写入校验码
            await stream.WriteByteAsync(HMetadataConstant.CCode);
            // 写入字段长度
            buffer = BitConverter.GetBytes(fields.Length);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            // 写入字段
            if (fields.Length > 0)
                await stream.WriteAsync(fields, 0, fields.Length);
            // 写入附加数据
            for (int i = 0; i < appendixLens.Length; i++)
            {
                // 写入校验码
                await stream.WriteByteAsync(HMetadataConstant.CCode);
                // 写入附加数据长度
                buffer = BitConverter.GetBytes(appendixLens[i]);
                await stream.WriteAsync(buffer, 0, buffer.Length);
                // 写入附加数据
                if (appendixes[i] is Stream)
                {
                    var s = appendixes[i] as Stream;
                    s.Seek(0, SeekOrigin.Begin);
                    await s.CopyToAsync(stream);
                }
                else if (appendixes[i] is byte[])
                {
                    var array = appendixes[i] as byte[];
                    await stream.WriteAsync(array, 0, array.Length);
                }
                else
                    throw new ArgumentException($"appendixes[{i}] only support Stream or byte[] type", "appendixes");
            }
            // 写入附加数据结尾
            await stream.WriteByteAsync(HMetadataConstant.CCode);
            buffer = BitConverter.GetBytes((int)0);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            // 写入校验码
            await stream.WriteByteAsync(HMetadataConstant.CCode);
            // 写入保留区长度
            if (FixedLength != HMetadataConstant.VariableLength)
                reserveLen = FixedLength - desiredSpace;

            buffer = BitConverter.GetBytes(reserveLen);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            // 填充保留区
            if (reserveLen > 0) await stream.FillAsync(HMetadataConstant.CCFlag, reserveLen);

            // 更新文件状态
            FileStatus.Position = position;
            FileStatus.FieldsLength = fields.Length;
            FileStatus.AppendixLengths = appendixLens;
            FileStatus.ReserveLength = reserveLen;
        }

        /// <summary>
        /// 从stream中的读取数据，会调用<see cref="SetFields(byte[])"/>
        /// </summary>
        /// <param name="stream">数据源</param>
        /// <param name="hasControlCode">数据源包涵控制码，需要检测控制码</param>
        public async Task LoadAsync(Stream stream)
        {
            ExceptionFactory.CheckArgNull("stream", stream);

            const int byteResultEnd = -1;
            int byteResult;
            // 数据段起始位置
            long position = stream.Position;
            // 字段数据长度
            int fieldsLen;
            // 附加数据长度
            List<int> appendixLens = new List<int>();
            // 保留区长度
            int reserveLen;
            // 字段缓存
            byte[] fields = null;
            // 验证控制码
            byteResult = await stream.ReadByteAsync();
            if (byteResult == byteResultEnd)
                throw new EndOfStreamException("Stream ended when read control code flag");

            if (HMetadataConstant.CCFlag != byteResult)
                throw new InvalidDataException($"Invalid control code flag: expected={HMetadataConstant.CCFlag}, value={byteResult}");

            byteResult = await stream.ReadByteAsync();
            if (byteResult == byteResultEnd)
                throw new EndOfStreamException("Stream ended when read control code");

            if (ControlCode != byteResult)
                throw new InvalidDataException($"Invalid control code: expected={ControlCode}, value={byteResult}");

            // 读取校验码
            byteResult = await stream.ReadByteAsync();
            if (byteResult == byteResultEnd)
                throw new EndOfStreamException("Stream ended when read check code of fields length");

            if (HMetadataConstant.CCode != byteResult)
                throw new InvalidDataException($"Invalid check code of fields length: value={byteResult}");

            byte[] intBuffer = new byte[4];
            // 读取字段数据长度
            if (intBuffer.Length != await stream.ReadAsync(intBuffer, 0, intBuffer.Length))
                throw new EndOfStreamException("Stream ended when read fields len");

            fieldsLen = BitConverter.ToInt32(intBuffer, 0);
            if (fieldsLen < 0)
                throw new InvalidDataException($"Invalid fields len: expected=[0,{int.MaxValue}] value={fieldsLen}");

            // 读取字段数据
            if (fieldsLen > 0)
            {
                fields = new byte[fieldsLen];
                if (fieldsLen != await stream.ReadAsync(fields, 0, fields.Length))
                    throw new EndOfStreamException("Stream ended when read fields");
            }

            while (true)
            {
                // 读取校验码
                byteResult = await stream.ReadByteAsync();
                if (byteResult == byteResultEnd)
                    throw new EndOfStreamException("Stream ended when read check code of appendix length");

                if (HMetadataConstant.CCode != byteResult)
                    throw new InvalidDataException($"Invalid check code of appendix length: value={byteResult}");

                // 读取附加数据长度
                if (intBuffer.Length != await stream.ReadAsync(intBuffer, 0, intBuffer.Length))
                    throw new EndOfStreamException("Stream ended when read appendix len");

                int appendixLen = BitConverter.ToInt32(intBuffer, 0);
                if (appendixLen < 0)
                    throw new InvalidDataException($"Invalid appendix len: expected=[0,{int.MaxValue}] value={appendixLen}");

                if (appendixLen == 0) break; // Readed the end of appendix

                appendixLens.Add(appendixLen);
                // 略过附加数据
                stream.Seek(appendixLen, SeekOrigin.Current);
            }
            // 读取校验码
            byteResult = await stream.ReadByteAsync();
            if (byteResult == byteResultEnd)
                throw new EndOfStreamException("Stream ended when read check code of reserved length");

            if (HMetadataConstant.CCode != byteResult)
                throw new InvalidDataException($"Invalid check code of reserved length: value={byteResult}");

            // 读取保留区长度
            if (intBuffer.Length != await stream.ReadAsync(intBuffer, 0, intBuffer.Length))
                throw new EndOfStreamException("Stream ended when read reserve len");

            reserveLen = BitConverter.ToInt32(intBuffer, 0);
            if (reserveLen < 0)
                throw new InvalidDataException($"Invalid reserve len: expected=[0,{int.MaxValue}] value={reserveLen}");

            // 略过保留区
            if (reserveLen > 0) stream.Seek(reserveLen, SeekOrigin.Current);

            // 更新文件状态
            FileStatus.Position = position;
            FileStatus.FieldsLength = fieldsLen;
            FileStatus.AppendixLengths = appendixLens.ToArray();
            FileStatus.ReserveLength = reserveLen;

            SetFields(fields);
        }

        /// <summary>
        /// 获取保存所有数据需要的长度
        /// </summary>
        /// <param name="appendixes">附加数据</param>
        /// <returns>需要的长度</returns>
        public int GetDesiredLength(object[] appendixes)
        {
            int CCLen = 2, CheckLen = 1, LenLen = 4;
            checked
            {
                // control code add fields
                int deired = CCLen + CheckLen + LenLen + GetFieldsLength();
                // appendix
                if (appendixes != null)
                {
                    for (int i = 0; i < appendixes.Length; i++)
                    {
                        long appendixLen = 0;
                        if (appendixes[i] is Stream)
                            appendixLen = (appendixes[i] as Stream).Length;
                        else if (appendixes[i] is byte[])
                            appendixLen = (appendixes[i] as byte[]).Length;
                        else
                            throw new ArgumentException($"appendixes[{i}] only support Stream or byte[] type", "appendixes");

                        if (appendixLen <= 0 || appendixLen > int.MaxValue)
                            throw new ArgumentException($"appendixes[{i}] length error: expected=[1,{int.MaxValue}], value={appendixLen}", "appendixes");

                        deired += CheckLen + LenLen + (int)appendixLen;
                    }
                }
                deired += CheckLen + LenLen; // Add the end of appendix: 0xFE 0x00 0x00 0x00 0x00
                // reserve
                deired += CheckLen + LenLen;
                return deired;
            }
        }

        /// <summary>
        /// 获取字段数据的长度
        /// </summary>
        /// <returns></returns>
        public abstract int GetFieldsLength();

        /// <summary>
        /// 深度复制
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            HMetadataSegment clone = MemberwiseClone() as HMetadataSegment;
            clone.FileStatus = FileStatus.Clone() as HMetadataSegmentFileStatus;
            OnClone(clone);
            return clone;
        }

        /// <summary>
        /// 深度复制，基础类字段已经通过MemberwiseClone函数复制，子类需要处理非基础类字段
        /// </summary>
        /// <param name="clone">副本</param>
        protected abstract void OnClone(HMetadataSegment clone);

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
