using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HMetadataPageContent : HMetadataSegment
    {
        public override byte ControlCode { get { return HMetadataControlCodes.PageContent; } }

        public bool HasThumbnail { get; set; }
        public const string HasThumbnailPropertyName = "HasThumbnail";

        public bool HasImage { get; set; }
        public const string HasImagePropertyName = "HasImage";

        public override int GetFieldsLength()
        {
            return 1 + 1;
        }

        protected override byte[] GetFields()
        {
            byte[] buffer = new byte[GetFieldsLength()];
            int writePos = 0;
            writePos += HMetadataHelper.WritePropertyBool(HasThumbnailPropertyName, HasThumbnail, buffer, writePos);
            writePos += HMetadataHelper.WritePropertyBool(HasImagePropertyName, HasImage, buffer, writePos);

            return buffer;
        }

        protected override void SetFields(byte[] buffer)
        {
            ExceptionFactory.CheckArgNull("buffer", buffer);

            int readPos = 0;
            bool hasThumbnail;
            readPos += HMetadataHelper.ReadPropertyBool(HasThumbnailPropertyName, out hasThumbnail, buffer, readPos);
            HasThumbnail = hasThumbnail;

            bool hasImg;
            readPos += HMetadataHelper.ReadPropertyBool(HasImagePropertyName, out hasImg, buffer, readPos);
            HasImage = hasImg;
        }

        protected override void OnClone(HMetadataSegment clone)
        {
        }

        public HMetadataAppendix GetThumbnail()
        {
            if (HasThumbnail) return FileStatus.GetAppendix(0);
            else return null;
        }

        public HMetadataAppendix GetImage()
        {
            if (!HasImage) return null;

            if (HasThumbnail) return FileStatus.GetAppendix(1);
            else return FileStatus.GetAppendix(0);
        }
    }
}
