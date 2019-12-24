using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Model
{
    public class CommonModifyInfo
    {
        public string Name { get; set; }

        public string StartVersion { get; set; }

        public string EndVersion { get; set; }

        public List<Change> Changes { get; set; }

        public CommonModifyInfo Clone()
        {
            CommonModifyInfo o = new CommonModifyInfo();
            o.Name = Name;
            o.StartVersion = StartVersion;
            o.EndVersion = EndVersion;
            List<Change> cs = new List<Change>();
            foreach(Change c in Changes)
            {
                cs.Add(c.Clone());
            }
            o.Changes = cs;
            return o;
        }
    }
}
