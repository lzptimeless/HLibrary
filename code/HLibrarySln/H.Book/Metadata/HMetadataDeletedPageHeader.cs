using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HMetadataDeletedPageHeader : HMetadataPageHeader
    {
        public override byte ControlCode { get { return HMetadataControlCodes.DeletedPageHeader; } }
    }
}
