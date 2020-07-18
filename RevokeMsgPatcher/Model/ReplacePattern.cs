using System;

namespace RevokeMsgPatcher.Model
{
    public class ReplacePattern
    {
        public byte[] Search { get; set; }

        public byte[] Replace { get; set; }

        public string Category { get; set; }

        public ReplacePattern Clone()
        {
            ReplacePattern o = new ReplacePattern();
            o.Search = Search;
            o.Replace = Replace;
            return o;
        }
    }
}
