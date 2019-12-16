using System.Diagnostics;

namespace RevokeMsgPatcher.MultiInstance
{
    public class WechatProcess
    {
        public Process Proc { get; set; }

        public bool MutexClosed { get; set; }

        public WechatProcess(Process p)
        {
            Proc = p;
            MutexClosed = false;
        }
    }
}
