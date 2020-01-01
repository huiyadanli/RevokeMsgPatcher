using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Model.Enum
{
    /// <summary>
    /// 两种打补丁的方式
    /// 精准（指定位置替换）、通用（特征码替换）
    /// </summary>
    public enum PatchType
    {
        Accurate,   // 精准（指定位置替换）
        Common      // 通用（特征码替换）
    }
}
