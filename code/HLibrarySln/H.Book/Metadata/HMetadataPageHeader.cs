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
        
        public string Artist { get; set; }
        public const string ArtistPropertyName = "Artist";
        public const int ArtistLen = 128;

        public string[] Characters { get; private set; }
        public const string CharactersPropertyName = "Characters";
        public const int CharactersItemLen = 128;

        public string[] Tags { get; private set; }
        public const string TagsPropertyName = "Tags";
        public const int TagsItemLen = 64;

        protected override int GetDataLength()
        {
            // 页内容位置+作者名+角色数+角色+标签数+标签
            return 4 + ArtistLen +
                1 + (Characters != null ? Characters.Length : 0) * CharactersItemLen +
                1 + (Tags != null ? Tags.Length : 0) * TagsItemLen;
        }

        protected override byte[] GetData()
        {
            ExceptionFactory.CheckPropertyCountRange(CharactersPropertyName, Characters, 0, byte.MaxValue);
            ExceptionFactory.CheckPropertyCountRange(TagsPropertyName, Tags, 0, byte.MaxValue);

            int dataLen = GetDataLength();
            byte[] data = new byte[dataLen];
            int writePos = 0;
            
            // 写入作者名
            writePos += HMetadataHelper.WritePropertyString(ArtistPropertyName, Artist, data, writePos, ArtistLen);
            // 写入角色
            writePos += HMetadataHelper.WritePropertyList(CharactersPropertyName, Characters, data, writePos, CharactersItemLen);
            // 写入标签
            writePos += HMetadataHelper.WritePropertyList(TagsPropertyName, Tags, data, writePos, TagsItemLen);

            return data;
        }

        protected override void LoadData(byte[] data)
        {
            ExceptionFactory.CheckArgNull("data", data);
            
            Artist = null;
            Characters = null;
            Tags = null;

            int readPos = 0;
            // 读取作者
            string artist;
            readPos += HMetadataHelper.ReadPropertyString(ArtistPropertyName, out artist, data, readPos, ArtistLen);
            Artist = artist;
            // 读取角色
            string[] characters;
            readPos += HMetadataHelper.ReadPropertyList(CharactersPropertyName, out characters, data, readPos, CharactersItemLen);
            Characters = characters;
            // 读取标签
            string[] tags;
            readPos += HMetadataHelper.ReadPropertyList(TagsPropertyName, out tags, data, readPos, TagsItemLen);
            Tags = tags;
        }
    }
}
