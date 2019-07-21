using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Model
{
    public class TargetFile
    {
        public string FileName { get; set; }

        public string Version { get; set; }

        public string SHA1Before { get; set; }

        public string SHA1After { get; set; }

        public long Position { get; set; }

        public byte Content { get; set; }
    }
}
