using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBook
{
    public class HBookIndex
    {
        public UInt16 PageCount { get; set; }
        public IList<UInt32> PagePositions { get; private set; }
    }
}
