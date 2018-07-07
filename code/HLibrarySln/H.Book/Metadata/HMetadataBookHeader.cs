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

        public override int FixedLength { get { return HMetadataConstant.BookHeaderLength; } }

        public byte Version { get; set; }
        public const string VersionPropertyName = "Version";

        /// <summary>
        /// Length is 16B
        /// </summary>
        public Guid ID { get; set; }
        public const string IDPropertyName = "ID";

        /// <summary>
        /// Length less than 32B
        /// </summary>
        public string IetfLanguageTag { get; set; }
        public const string IetfLanguageTagPropertyName = "IetfLanguageTag";

        /// <summary>
        /// Each name length less that 128B
        /// </summary>
        public string[] Names { get; set; }
        public const string NamesPropertyName = "Names";

        /// <summary>
        /// Each artist length less that 128B
        /// </summary>
        public string[] Artists { get; set; }
        public const string ArtistsPropertyName = "Artists";

        /// <summary>
        /// Each group length less that 128B
        /// </summary>
        public string[] Groups { get; set; }
        public const string GroupsPropertyName = "Groups";

        /// <summary>
        /// Each series length less that 128B
        /// </summary>
        public string[] Series { get; set; }
        public const string SeriesPropertyName = "Series";

        /// <summary>
        /// Each category length less that 128B
        /// </summary>
        public string[] Categories { get; set; }
        public const string CategoriesPropertyName = "Categories";

        /// <summary>
        /// Each character length less that 128B
        /// </summary>
        public string[] Characters { get; set; }
        public const string CharactersPropertyName = "Characters";

        /// <summary>
        /// Each rag length less that 64B
        /// </summary>
        public string[] Tags { get; set; }
        public const string TagsPropertyName = "Tags";

        public override int GetFieldsLength()
        {
            // 版本+ID+语言+书名+作者+分组+系列+分类+角色+标签
            int dataLen = 1 + 16 + HMetadataHelper.GetByteLen(IetfLanguageTag) +
                HMetadataHelper.GetByteLen(Names) +
                HMetadataHelper.GetByteLen(Artists) +
                HMetadataHelper.GetByteLen(Groups) +
                HMetadataHelper.GetByteLen(Series) +
                HMetadataHelper.GetByteLen(Categories) +
                HMetadataHelper.GetByteLen(Characters) +
                HMetadataHelper.GetByteLen(Tags);
            return dataLen;
        }

        protected override byte[] GetFields()
        {
            ExceptionFactory.CheckPropertyEmpty(IDPropertyName, ID);
            ExceptionFactory.CheckArgCountRange(NamesPropertyName, Names, 0, byte.MaxValue);
            ExceptionFactory.CheckArgCountRange(ArtistsPropertyName, Artists, 0, byte.MaxValue);
            ExceptionFactory.CheckArgCountRange(GroupsPropertyName, Groups, 0, byte.MaxValue);
            ExceptionFactory.CheckArgCountRange(SeriesPropertyName, Series, 0, byte.MaxValue);
            ExceptionFactory.CheckArgCountRange(CategoriesPropertyName, Categories, 0, byte.MaxValue);
            ExceptionFactory.CheckArgCountRange(CharactersPropertyName, Characters, 0, byte.MaxValue);
            ExceptionFactory.CheckArgCountRange(TagsPropertyName, Tags, 0, byte.MaxValue);

            int dataLen = GetFieldsLength();
            byte[] data = new byte[dataLen];
            int writePos = 0;

            // 写入版本
            data[writePos] = Version;
            writePos += 1;
            // 写入ID
            writePos += HMetadataHelper.WritePropertyGuid(IDPropertyName, ID, data, writePos);
            // 写入语言
            writePos += HMetadataHelper.WritePropertyString(IetfLanguageTagPropertyName, IetfLanguageTag, data, writePos);
            // 写入书名数,书名
            writePos += HMetadataHelper.WritePropertyList(NamesPropertyName, Names, data, writePos);
            // 写入作者数,作者
            writePos += HMetadataHelper.WritePropertyList(ArtistsPropertyName, Artists, data, writePos);
            // 写入分组数,分组
            writePos += HMetadataHelper.WritePropertyList(GroupsPropertyName, Groups, data, writePos);
            // 写入系列数,系列
            writePos += HMetadataHelper.WritePropertyList(SeriesPropertyName, Series, data, writePos);
            // 写入分类数,分类
            writePos += HMetadataHelper.WritePropertyList(CategoriesPropertyName, Categories, data, writePos);
            // 写入角色数,角色
            writePos += HMetadataHelper.WritePropertyList(CharactersPropertyName, Characters, data, writePos);
            // 写入标签数,标签
            writePos += HMetadataHelper.WritePropertyList(TagsPropertyName, Tags, data, writePos);

            if (writePos != dataLen)
                throw new WritePropertyException("Unkown", $"Some error occurred in write property: writePos={writePos}, dataLen={dataLen}", null);

            return data;
        }

        protected override void SetFields(byte[] buffer)
        {
            ExceptionFactory.CheckArgNull("buffer", buffer);

            Version = 0;
            ID = Guid.Empty;
            IetfLanguageTag = string.Empty;
            Names = null;
            Artists = null;
            Groups = null;
            Series = null;
            Categories = null;
            Characters = null;
            Tags = null;

            int readPos = 0;
            // 读取版本
            Version = buffer[readPos];
            readPos++;
            // 读取ID
            Guid id;
            readPos += HMetadataHelper.ReadPropertyGuid(IDPropertyName, out id, buffer, readPos);
            ID = id;
            ExceptionFactory.CheckPropertyEmpty(IDPropertyName, id);
            // 读取语言
            string language;
            readPos += HMetadataHelper.ReadPropertyString(IetfLanguageTagPropertyName, out language, buffer, readPos);
            IetfLanguageTag = language;
            // 读取书名数,书名
            string[] names;
            readPos += HMetadataHelper.ReadPropertyList(NamesPropertyName, out names, buffer, readPos);
            Names = names;
            // 读取作者数,作者
            string[] artists;
            readPos += HMetadataHelper.ReadPropertyList(ArtistsPropertyName, out artists, buffer, readPos);
            Artists = artists;
            // 读取分组数,分组
            string[] groups;
            readPos += HMetadataHelper.ReadPropertyList(GroupsPropertyName, out groups, buffer, readPos);
            Groups = groups;
            // 读取系列数,系列
            string[] series;
            readPos += HMetadataHelper.ReadPropertyList(SeriesPropertyName, out series, buffer, readPos);
            Series = series;
            // 读取分类数,分类
            string[] categories;
            readPos += HMetadataHelper.ReadPropertyList(CategoriesPropertyName, out categories, buffer, readPos);
            Categories = categories;
            // 读取角色数,角色
            string[] characters;
            readPos += HMetadataHelper.ReadPropertyList(CharactersPropertyName, out characters, buffer, readPos);
            Characters = characters;
            // 读取标签数,标签
            string[] tags;
            readPos += HMetadataHelper.ReadPropertyList(TagsPropertyName, out tags, buffer, readPos);
            Tags = tags;
        }

        protected override void OnClone(HMetadataSegment clone)
        {
        }
    }
}
