using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Model
{
    public class ModifyInfo
    {
        public string Name { get; set; }

        //public string RelativePath { get; set; }

        public string Version { get; set; }

        public string SHA1Before { get; set; }

        public string SHA1After { get; set; }

        public List<Change> Changes { get; set; }

        public ModifyInfo Clone()
        {
            ModifyInfo o = new ModifyInfo();
            o.Name = Name;
            o.Version = Version;
            o.SHA1Before = SHA1Before;
            o.SHA1After = SHA1After;
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
