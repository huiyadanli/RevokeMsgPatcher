using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Model
{
    public class TargetInfo
    {
        public string Name { get; set; }

        public string RelativePath { get; set; }

        public string Memo { get; set; }

        public string StartVersion { get; set; }

        public string EndVersion { get; set; }

        public TargetInfo Clone()
        {
            TargetInfo o = new TargetInfo();
            o.Name = Name;
            o.RelativePath = RelativePath;
            o.Memo = Memo;
            return o;
        }
    }
}
