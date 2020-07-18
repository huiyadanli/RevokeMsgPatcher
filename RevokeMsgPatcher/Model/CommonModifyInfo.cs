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

        public List<ReplacePattern> ReplacePatterns { get; set; }

        public CommonModifyInfo Clone()
        {
            CommonModifyInfo o = new CommonModifyInfo();
            o.Name = Name;
            o.StartVersion = StartVersion;
            o.EndVersion = EndVersion;
            List<ReplacePattern> cs = new List<ReplacePattern>();
            foreach (ReplacePattern c in ReplacePatterns)
            {
                cs.Add(c.Clone());
            }
            o.ReplacePatterns = cs;
            return o;
        }

        public List<string> GetCategories()
        {
            if (ReplacePatterns != null && ReplacePatterns.Count > 0)
            {
                return ReplacePatterns.Select(p => p.Category).ToList();
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
