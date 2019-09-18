using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Model
{
    public class BinaryFile
    {
        //public string Name { get {return Path.GetFileName(RelativePath); } }

        public string RelativePath { get; set; }

        public string Version { get; set; }

        public string SHA1Before { get; set; }

        public string SHA1After { get; set; }

        public List<Change> Changes { get; set; }
    }
}
