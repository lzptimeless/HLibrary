using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class HMetadataAppendix
    {
        public HMetadataAppendix(long position, int length)
        {
            Position = position;
            Length = length;
        }

        public long Position { get; private set; }
        public int Length { get; private set; }
    }
}
