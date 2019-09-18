using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Model
{
    public class Version
    {
        public string AppVersion { get; set; }

        public List<ModifyInfo> Files { get; set; }
    }
}
