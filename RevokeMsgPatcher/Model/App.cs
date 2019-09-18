using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Model
{
    public class App
    {
        public string Name { get; set; }

        public string[] ModifyFilePaths { get; set; }

        public List<BinaryFile> ModifyFileInfos { get; set; }
    }
}
