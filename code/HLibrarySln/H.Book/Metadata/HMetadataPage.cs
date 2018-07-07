using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HMetadataPage
    {
        public HMetadataPage(HMetadataPageHeader headerMetadata, HMetadataPageContent contentMetadata)
        {
            HeaderMetadata = headerMetadata;
            ContentMetadata = contentMetadata;
        }

        public HMetadataPageHeader HeaderMetadata { get; private set; }
        public HMetadataPageContent ContentMetadata { get; set; }
    }
}
