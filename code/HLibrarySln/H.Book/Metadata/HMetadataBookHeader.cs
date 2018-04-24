using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HMetadataBookHeader : HMetadataSegment
    {
        public override byte ControlCode { get { return HMetadataControlCodes.BookHeader; } }
        public byte Version { get; set; }
        /// <summary>
        /// Length is 128B
        /// </summary>
        public Guid ID { get; set; }
        public int CoverPosition { get; set; }
        public int IndexPosition { get; set; }
        /// <summary>
        /// Length less than 32B
        /// </summary>
        public string IetfLanguageTag { get; set; }
        /// <summary>
        /// Each name length less that 128B
        /// </summary>
        public IList<string> Names { get; private set; }
        /// <summary>
        /// Each artist length less that 128B
        /// </summary>
        public IList<string> Artists { get; private set; }
        /// <summary>
        /// Each group length less that 128B
        /// </summary>
        public IList<string> Groups { get; private set; }
        /// <summary>
        /// Each series length less that 128B
        /// </summary>
        public IList<string> Series { get; private set; }
        /// <summary>
        /// Each category length less that 128B
        /// </summary>
        public IList<string> Categories { get; private set; }
        /// <summary>
        /// Each character length less that 128B
        /// </summary>
        public IList<string> Characters { get; private set; }
        /// <summary>
        /// Each rag length less that 64B
        /// </summary>
        public IList<string> Tags { get; private set; }

        protected override int GetDataLength()
        {
            // 版本+ID+封面位置+索引位置+语言+书名数+书名+作者数+作者+分组数+分组+系列数+系列+分类数+分类+角色数+角色+标签数+标签
            int dataLen = 1 + 128 + 4 + 4 + 32 +
                1 + (Names != null ? Names.Count * 128 : 0) +
                1 + (Artists != null ? Artists.Count * 128 : 0) +
                1 + (Groups != null ? Groups.Count * 128 : 0) +
                1 + (Series != null ? Series.Count * 128 : 0) +
                1 + (Categories != null ? Categories.Count * 128 : 0) +
                1 + (Characters != null ? Characters.Count * 128 : 0) +
                1 + (Tags != null ? Tags.Count * 64 : 0);
            return dataLen;
        }

        protected override byte[] GetData()
        {
            if (Guid.Empty == ID)
                throw new ApplicationException("ID can not be empty");

            if (CoverPosition < 0)
                throw new ApplicationException($"Invalid CoverPosition: expected=[0,{int.MaxValue}], value={CoverPosition}");

            if (IndexPosition < 0)
                throw new ApplicationException($"Invalid IndexPosition: expected=[0,{int.MaxValue}], value={IndexPosition}");

            CheckListMaxCount(Names, byte.MaxValue, "Names");
            CheckListMaxCount(Artists, byte.MaxValue, "Artists");
            CheckListMaxCount(Groups, byte.MaxValue, "Groups");
            CheckListMaxCount(Series, byte.MaxValue, "Series");
            CheckListMaxCount(Categories, byte.MaxValue, "Categories");
            CheckListMaxCount(Characters, byte.MaxValue, "Characters");
            CheckListMaxCount(Tags, byte.MaxValue, "Tags");

            int dataLen = GetDataLength();
            byte[] data = new byte[dataLen];
            byte[] buffer;
            int writePosition = 0;

            // 写入版本
            data[writePosition] = Version;
            writePosition++;
            // 写入ID
            buffer = ID.ToByteArray();
            CheckBytesLength(buffer, 128, "ID");
            Array.Copy(buffer, 0, data, writePosition, buffer.Length);
            writePosition += buffer.Length;
            // 写入封面位置
            buffer = BitConverter.GetBytes(CoverPosition);
            CheckBytesLength(buffer, 4, "CoverPosition");
            Array.Copy(buffer, 0, data, writePosition, buffer.Length);
            writePosition += buffer.Length;
            // 写入索引位置
            buffer = BitConverter.GetBytes(IndexPosition);
            CheckBytesLength(buffer, 4, "IndexPosition");
            Array.Copy(buffer, 0, data, writePosition, buffer.Length);
            writePosition += buffer.Length;
            // 写入语言
            buffer = GetStringBytes(IetfLanguageTag, 32);
            Array.Copy(buffer, 0, data, writePosition, buffer.Length);
            writePosition += buffer.Length;
            // 写入书名数,书名
            writePosition += WriteStringListProperty(Names, 128, data, writePosition);
            // 写入作者数,作者
            writePosition += WriteStringListProperty(Artists, 128, data, writePosition);
            // 写入分组数,分组
            writePosition += WriteStringListProperty(Groups, 128, data, writePosition);
            // 写入系列数,系列
            writePosition += WriteStringListProperty(Series, 128, data, writePosition);
            // 写入分类数,分类
            writePosition += WriteStringListProperty(Categories, 128, data, writePosition);
            // 写入角色数,角色
            writePosition += WriteStringListProperty(Characters, 128, data, writePosition);
            // 写入标签数,标签
            writePosition += WriteStringListProperty(Tags, 64, data, writePosition);

            return data;
        }

        protected override void LoadData(byte[] data)
        {
            throw new NotImplementedException();
        }

        private int WriteStringListProperty(IList<string> list, int itemBytesLen, byte[] destination, int startIndex)
        {
            ExceptionFactory.CheckArgRange(itemBytesLen, 1, int.MaxValue, "itemBytesLen");
            ExceptionFactory.CheckNull(destination, "destination");
            ExceptionFactory.CheckArgRange(startIndex, 0, destination.Length, "startIndex");

            int itemCount = list != null ? list.Count : 0;
            int writePosition = startIndex;
            destination[writePosition] = (byte)itemCount;
            writePosition++;

            if (itemCount > 0)
            {
                byte[] buffer;
                foreach (var s in Names)
                {
                    buffer = GetStringBytes(s, itemBytesLen);
                    Array.Copy(buffer, 0, destination, writePosition, buffer.Length);
                    writePosition += buffer.Length;
                }
            }

            return writePosition - startIndex;
        }

        private byte[] GetStringBytes(string s, int desireLen)
        {
            if (desireLen <= 0)
                throw new ArgumentOutOfRangeException("desireLen", $"Invalid desireLen: expected=[1,{int.MaxValue}], value={desireLen}");

            byte[] bytes = new byte[desireLen];
            Encoding.UTF8.GetBytes(s, 0, s.Length, bytes, 0);

            return bytes;
        }

        private void CheckListMaxCount<T>(IList<T> list, int maxCount, string listName)
        {
            if (list == null)
                return;

            if (list.Count > maxCount)
                throw new ApplicationException($"{listName} count valid: expected=[0,{maxCount}], value={list.Count}");
        }

        private void CheckBytesLength(byte[] bytes, int desiredLen, string dataName)
        {
            if (bytes.Length != desiredLen)
                throw new ApplicationException($"{dataName} bytes length invalid: expected={desiredLen}, value={bytes.Length}");
        }
    }
}
