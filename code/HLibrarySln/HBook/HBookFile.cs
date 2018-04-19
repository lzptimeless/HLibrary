﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBook
{
    public class HBookFile
    {
        public HMetadataBookHeader Header { get; set; }
        public HBookCover Cover { get; set; }
        public HBookIndex Index { get; set; }
        public IList<HBookPage> Pages { get; private set; }
        public IList<HBookVirtualPage> VirtualPages { get; private set; }

        public void Load(string path)
        { }

        public void Save()
        { }

        public void SaveAs(string path)
        { }
    }
}
