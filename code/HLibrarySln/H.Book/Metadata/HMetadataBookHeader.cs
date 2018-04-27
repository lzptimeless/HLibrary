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
        public const string VersionPropertyName = "Version";

        /// <summary>
        /// Length is 16B
        /// </summary>
        public Guid ID { get; set; }
        public const string IDPropertyName = "ID";

        public int CoverPosition { get; set; }
        public const string CoverPositionPropertyName = "CoverPosition";

        public int IndexPosition { get; set; }
        public const string IndexPositionPropertyName = "IndexPosition";

        /// <summary>
        /// Length less than 32B
        /// </summary>
        public string IetfLanguageTag { get; set; }
        public const string IetfLanguageTagPropertyName = "IetfLanguageTag";
        protected const int IetfLanguageTagLen = 32;

        /// <summary>
        /// Each name length less that 128B
        /// </summary>
        public IList<string> Names { get; private set; }
        public const string NamesPropertyName = "Names";
        protected const int NamesItemLen = 128;

        /// <summary>
        /// Each artist length less that 128B
        /// </summary>
        public IList<string> Artists { get; private set; }
        public const string ArtistsPropertyName = "Artists";
        protected const int ArtistsItemLen = 128;

        /// <summary>
        /// Each group length less that 128B
        /// </summary>
        public IList<string> Groups { get; private set; }
        public const string GroupsPropertyName = "Groups";
        protected const int GroupsItemLen = 128;

        /// <summary>
        /// Each series length less that 128B
        /// </summary>
        public IList<string> Series { get; private set; }
        public const string SeriesPropertyName = "Series";
        protected const int SeriesItemLen = 128;

        /// <summary>
        /// Each category length less that 128B
        /// </summary>
        public IList<string> Categories { get; private set; }
        public const string CategoriesPropertyName = "Categories";
        protected const int CategoriesItemLen = 128;

        /// <summary>
        /// Each character length less that 128B
        /// </summary>
        public IList<string> Characters { get; private set; }
        public const string CharactersPropertyName = "Characters";
        protected const int CharactersItemLen = 128;

        /// <summary>
        /// Each rag length less that 64B
        /// </summary>
        public IList<string> Tags { get; private set; }
        public const string TagsPropertyName = "Tags";
        protected const int TagsItemLen = 64;

        protected override int GetDataLength()
        {
            // 版本+ID+封面位置+索引位置+语言+书名数+书名+作者数+作者+分组数+分组+系列数+系列+分类数+分类+角色数+角色+标签数+标签
            int dataLen = 1 + 128 + 4 + 4 + IetfLanguageTagLen +
                1 + (Names != null ? Names.Count * NamesItemLen : 0) +
                1 + (Artists != null ? Artists.Count * ArtistsItemLen : 0) +
                1 + (Groups != null ? Groups.Count * GroupsItemLen : 0) +
                1 + (Series != null ? Series.Count * SeriesItemLen : 0) +
                1 + (Categories != null ? Categories.Count * CategoriesItemLen : 0) +
                1 + (Characters != null ? Characters.Count * CharactersItemLen : 0) +
                1 + (Tags != null ? Tags.Count * TagsItemLen : 0);
            return dataLen;
        }

        protected override byte[] GetData()
        {
            ExceptionFactory.CheckPropertyEmpty(IDPropertyName, ID);
            ExceptionFactory.CheckPropertyRange(CoverPositionPropertyName, CoverPosition, 0, int.MaxValue);
            ExceptionFactory.CheckPropertyRange(IndexPositionPropertyName, IndexPosition, 0, int.MaxValue);
            ExceptionFactory.CheckArgCountRange(NamesPropertyName, Names, 0, byte.MaxValue);
            ExceptionFactory.CheckArgCountRange(ArtistsPropertyName, Artists, 0, byte.MaxValue);
            ExceptionFactory.CheckArgCountRange(GroupsPropertyName, Groups, 0, byte.MaxValue);
            ExceptionFactory.CheckArgCountRange(SeriesPropertyName, Series, 0, byte.MaxValue);
            ExceptionFactory.CheckArgCountRange(CategoriesPropertyName, Categories, 0, byte.MaxValue);
            ExceptionFactory.CheckArgCountRange(CharactersPropertyName, Characters, 0, byte.MaxValue);
            ExceptionFactory.CheckArgCountRange(TagsPropertyName, Tags, 0, byte.MaxValue);

            int dataLen = GetDataLength();
            byte[] data = new byte[dataLen];
            int writePos = 0;

            // 写入版本
            writePos += HMetadataHelper.WriteProperty(VersionPropertyName, Version, data, writePos);
            // 写入ID
            writePos += HMetadataHelper.WriteProperty(IDPropertyName, ID, data, writePos);
            // 写入封面位置
            writePos += HMetadataHelper.WriteProperty(CoverPositionPropertyName, CoverPosition, data, writePos);
            // 写入索引位置
            writePos += HMetadataHelper.WriteProperty(IndexPositionPropertyName, IndexPosition, data, writePos);
            // 写入语言
            writePos += HMetadataHelper.WriteProperty(IetfLanguageTagPropertyName, IetfLanguageTag, data, writePos, IetfLanguageTagLen);
            // 写入书名数,书名
            writePos += HMetadataHelper.WriteProperty(NamesPropertyName, Names, data, writePos, NamesItemLen);
            // 写入作者数,作者
            writePos += HMetadataHelper.WriteProperty(ArtistsPropertyName, Artists, data, writePos, ArtistsItemLen);
            // 写入分组数,分组
            writePos += HMetadataHelper.WriteProperty(GroupsPropertyName, Groups, data, writePos, GroupsItemLen);
            // 写入系列数,系列
            writePos += HMetadataHelper.WriteProperty(SeriesPropertyName, Series, data, writePos, SeriesItemLen);
            // 写入分类数,分类
            writePos += HMetadataHelper.WriteProperty(CategoriesPropertyName, Categories, data, writePos, CategoriesItemLen);
            // 写入角色数,角色
            writePos += HMetadataHelper.WriteProperty(CharactersPropertyName, Characters, data, writePos, CharactersItemLen);
            // 写入标签数,标签
            writePos += HMetadataHelper.WriteProperty(TagsPropertyName, Tags, data, writePos, TagsItemLen);

            return data;
        }

        protected override void LoadData(byte[] data)
        {
            Version = 0;
            ID = Guid.Empty;
            CoverPosition = 0;
            IndexPosition = 0;
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
            Version = data[readPos];
            readPos++;
            // 读取ID
            Guid id;
            readPos += HMetadataHelper.ReadProperty(IDPropertyName, out id, data, readPos);
            ID = id;
            // 读取封面位置
            int coverPosition;
            readPos += HMetadataHelper.ReadProperty(CoverPositionPropertyName, out coverPosition, data, readPos);
            CoverPosition = coverPosition;
            // 读取索引位置
            int indexPosition;
            readPos += HMetadataHelper.ReadProperty(IndexPositionPropertyName, out indexPosition, data, readPos);
            IndexPosition = indexPosition;
            // 读取语言
            string language;
            readPos += HMetadataHelper.ReadProperty(IetfLanguageTagPropertyName, out language, data, readPos, IetfLanguageTagLen);
            IetfLanguageTag = language;
            // 读取书名数,书名
            List<string> names;
            readPos += HMetadataHelper.ReadProperty(NamesPropertyName, out names, data, readPos, NamesItemLen);
            Names = names;
            // 读取作者数,作者
            List<string> artists;
            readPos += HMetadataHelper.ReadProperty(ArtistsPropertyName, out artists, data, readPos, ArtistsItemLen);
            Artists = artists;
            // 读取分组数,分组
            List<string> groups;
            readPos += HMetadataHelper.ReadProperty(GroupsPropertyName, out groups, data, readPos, GroupsItemLen);
            Groups = groups;
            // 读取系列数,系列
            List<string> series;
            readPos += HMetadataHelper.ReadProperty(SeriesPropertyName, out series, data, readPos, SeriesItemLen);
            Series = series;
            // 读取分类数,分类
            List<string> categories;
            readPos += HMetadataHelper.ReadProperty(CategoriesPropertyName, out categories, data, readPos, CategoriesItemLen);
            Categories = categories;
            // 读取角色数,角色
            List<string> characters;
            readPos += HMetadataHelper.ReadProperty(CharactersPropertyName, out characters, data, readPos, CharactersItemLen);
            Characters = characters;
            // 读取标签数,标签
            List<string> tags;
            readPos += HMetadataHelper.ReadProperty(TagsPropertyName, out tags, data, readPos, TagsItemLen);
            Tags = tags;
        }
    }
}
