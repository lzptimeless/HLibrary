using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HMetadataPageHeader : HMetadataSegment
    {
        public override byte ControlCode { get { return HMetadataControlCodes.PageHeader; } }

        public override int FixedLength { get { return HMetadataConstant.PageHeaderLength; } }

        /// <summary>
        /// Length is 16B
        /// </summary>
        public Guid ID { get; set; }
        public const string IDPropertyName = "ID";
        /// <summary>
        /// 页面内容的起始位置，大小8B
        /// </summary>
        public long ContentPosition { get; set; }
        public const string ContentPositionPropertyName = "ContentPosition";
        /// <summary>
        /// 这个页面是否已经删除了
        /// </summary>
        public bool IsDeleted { get; set; }
        public const string IsDeletedPropertyName = "IsDeleted";

        public string Artist { get; set; }
        public const string ArtistPropertyName = "Artist";

        public string[] Characters { get; set; }
        public const string CharactersPropertyName = "Characters";

        public string[] Tags { get; set; }
        public const string TagsPropertyName = "Tags";

        public override int GetFieldsLength()
        {
            // 页面ID+页内容位置+作者名+角色+标签
            return 16 + 8 + 1 + HMetadataHelper.GetByteLen(Artist) +
                HMetadataHelper.GetByteLen(Characters) +
                HMetadataHelper.GetByteLen(Tags);
        }

        protected override byte[] GetFields()
        {
            ExceptionFactory.CheckPropertyEmpty(IDPropertyName, ID);
            ExceptionFactory.CheckPropertyCountRange(CharactersPropertyName, Characters, 0, byte.MaxValue);
            ExceptionFactory.CheckPropertyCountRange(TagsPropertyName, Tags, 0, byte.MaxValue);

            int dataLen = GetFieldsLength();
            byte[] data = new byte[dataLen];
            int writePos = 0;
            // 写入ID
            writePos += HMetadataHelper.WritePropertyGuid(IDPropertyName, ID, data, writePos);
            // 写入内容起始位置
            writePos += HMetadataHelper.WritePropertyLong(ContentPositionPropertyName, ContentPosition, data, writePos);
            // 写入删除标记
            writePos += HMetadataHelper.WritePropertyBool(IsDeletedPropertyName, IsDeleted, data, writePos);
            // 写入作者名
            writePos += HMetadataHelper.WritePropertyString(ArtistPropertyName, Artist, data, writePos);
            // 写入角色
            writePos += HMetadataHelper.WritePropertyList(CharactersPropertyName, Characters, data, writePos);
            // 写入标签
            writePos += HMetadataHelper.WritePropertyList(TagsPropertyName, Tags, data, writePos);

            if (writePos != dataLen)
                throw new WritePropertyException("Unkown", $"Some error occurred in write property: writePos={writePos}, dataLen={dataLen}", null);

            return data;
        }

        protected override void SetFields(byte[] buffer)
        {
            ExceptionFactory.CheckArgNull("buffer", buffer);

            Artist = null;
            Characters = null;
            Tags = null;

            int readPos = 0;
            // 读取ID
            Guid id;
            readPos += HMetadataHelper.ReadPropertyGuid(IDPropertyName, out id, buffer, readPos);
            ID = id;
            // 读取内容位置
            long contentPos;
            readPos += HMetadataHelper.ReadPropertyLong(ContentPositionPropertyName, out contentPos, buffer, readPos);
            ContentPosition = contentPos;
            // 读取删除标记
            bool isDeleted;
            readPos += HMetadataHelper.ReadPropertyBool(IsDeletedPropertyName, out isDeleted, buffer, readPos);
            IsDeleted = isDeleted;
            // 读取作者
            string artist;
            readPos += HMetadataHelper.ReadPropertyString(ArtistPropertyName, out artist, buffer, readPos);
            Artist = artist;
            // 读取角色
            string[] characters;
            readPos += HMetadataHelper.ReadPropertyList(CharactersPropertyName, out characters, buffer, readPos);
            Characters = characters;
            // 读取标签
            string[] tags;
            readPos += HMetadataHelper.ReadPropertyList(TagsPropertyName, out tags, buffer, readPos);
            Tags = tags;
        }

        protected override void OnClone(HMetadataSegment clone)
        {
        }
    }
}
