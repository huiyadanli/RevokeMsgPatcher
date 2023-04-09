using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Model
{
    public class Bag
    {
        public Dictionary<string, App> Apps { get; set; }

        public string LatestVersion { get; set; }

        public string Notice { get; set; }

        public string NoticeUrl { get; set; }

        public int PatchVersion { get; set; }
    }
}
