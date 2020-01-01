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

        public byte[] Content { get; set; }

        public Change()
        {

        }

        public Change(long position, byte[] content)
        {
            Position = position;
            Content = content;
        }

        public Change Clone()
        {
            Change o = new Change();
            o.Position = Position;
            o.Content = Content;
            return o;
        }
    }
}
