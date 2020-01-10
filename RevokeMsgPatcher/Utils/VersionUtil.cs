using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Utils
{
    public class VersionUtil
    {
        /// <summary>
        /// 版本比较
        /// 0  相等
        /// 1  大与
        /// -1 小于
        /// 为空的版本最大
        /// </summary>
        /// <param name="v1">版本1</param>
        /// <param name="v2">版本2</param>
        /// <returns></returns>
        public static int Compare(string v1, string v2)
        {
            if (string.IsNullOrEmpty(v1))
            {
                return 1;
            }
            if (string.IsNullOrEmpty(v2))
            {
                return -1;
            }

            string[] v1s = v1.Split('.');
            string[] v2s = v2.Split('.');
            int len = Math.Max(v1s.Length, v2s.Length);
            for (int i = 0; i < len; i++)
            {
                int i1 = 0, i2 = 0;
                if (i < v1s.Length)
                {
                    i1 = Convert.ToInt32(v1s[i]);
                }
                if (i < v2s.Length)
                {
                    i2 = Convert.ToInt32(v2s[i]);
                }
                if (i1 == i2)
                {
                    continue;
                }
                else if (i1 > i2)
                {
                    return 1;
                }
                else if (i1 < i2)
                {
                    return -1;
                }
            }
            return 0;
        }

    }
}
