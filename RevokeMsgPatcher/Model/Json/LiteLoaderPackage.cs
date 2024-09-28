using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Model.Json
{
    internal class LiteLoaderPackage
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool Private { get; set; }
        public string Description { get; set; }
        public string ProductName { get; set; }
        public string Homepage { get; set; }
        public bool SideEffects { get; set; }
        public string Main { get; set; }
        public string BuildVersion { get; set; }
        public bool IsPureShell { get; set; }
        public bool IsByteCodeShell { get; set; }
        public string Platform { get; set; }
        public string EleArch { get; set; }
    }
}
