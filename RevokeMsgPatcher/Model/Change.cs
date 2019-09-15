using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Model
{
    public class Change
    {
        public long Position { get; set; }

        public byte Content { get; set; }
    }
}
