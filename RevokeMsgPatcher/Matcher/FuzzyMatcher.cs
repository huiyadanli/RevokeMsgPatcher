using System;
using System.Collections.Generic;

namespace RevokeMsgPatcher.Matcher
{
    /// <summary>
    /// 对16进制数据进行通配符查找
    /// </summary>
    public class FuzzyMatcher
    {
        public const byte wildcard = 0x3F; // 通配符

        /// <summary>
        /// 通配符匹配所有符合查找串的位置
        /// </summary>
        /// <param name="content">被查找对象</param>
        /// <param name="pattern">查找串</param>
        /// <returns></returns>
        public static int[] MatchAll(byte[] content, byte[] pattern)
        {
            return new List<int>(EnumerateMatches(content, pattern)).ToArray();
        }

        /// <summary>
        /// 仅查找首个匹配位置，适用于判断特征是否存在。
        /// </summary>
        public static bool TryMatch(byte[] content, byte[] pattern, out int firstShift)
        {
            foreach (int index in EnumerateMatches(content, pattern))
            {
                firstShift = index;
                return true;
            }
            firstShift = -1;
            return false;
        }

        private static IEnumerable<int> EnumerateMatches(byte[] content, byte[] pattern)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            int headOffset;
            byte[] head = GetHead(pattern, out headOffset);
            // 只扫描能够容纳完整查找串的范围，排除文件尾部的不完整候选。
            int lastStart = content.Length - pattern.Length;
            // 头串和查找串相同则直接返回，不同则继续判断是否符合查询串
            foreach (int headIndex in BoyerMooreMatcher.EnumerateMatches(content, head, headOffset, lastStart + headOffset))
            {
                int index = headIndex - headOffset;
                if (head.Length == pattern.Length || IsEqual(content, index, pattern))
                {
                    yield return index;
                }
            }
        }

        /// <summary>
        /// 通配符匹配所有符合查找串的位置，并排除已经替换的情况
        /// </summary>
        /// <param name="content">被查找对象</param>
        /// <param name="searchBytes">查找串</param>
        /// <param name="replaceBytes">替换串</param>
        /// <returns></returns>
        public static int[] MatchNotReplaced(byte[] content, byte[] searchBytes, byte[] replaceBytes)
        {
            if (replaceBytes == null)
            {
                throw new ArgumentNullException("replaceBytes");
            }
            // 头串和查找串相同则直接返回，不同则继续判断是否符合查询串
            // 完整查找串的校验由枚举器完成，不再创建中间候选数组。
            List<int> res = new List<int>();
            // 判断是否与替换串相同
            foreach (int index in EnumerateMatches(content, searchBytes))
            {
                if (!IsEqual(content, index, replaceBytes))
                {
                    res.Add(index);
                }
            }
            return res.ToArray();
        }

        /// <summary>
        /// 获取头串（选用最长连续固定片段作为定位串）
        /// </summary>
        /// <param name="whole">完整查找串</param>
        /// <param name="offset">定位串在完整查找串内的偏移</param>
        /// <returns></returns>
        private static byte[] GetHead(byte[] whole, out int offset)
        {
            if (whole == null)
            {
                throw new ArgumentNullException("whole");
            }
            if (whole.Length == 0)
            {
                throw new ArgumentException("查找串不能为空。", "whole");
            }
            if (whole[0] == wildcard)
            {
                throw new ArgumentException("不正确的通配符位置!", "whole");
            }
            // 原先只使用首个通配符前的头串；现在选取最长连续固定片段，减少候选数量。
            offset = 0;
            int len = 0;
            int runStart = 0;
            for (int i = 0; i < whole.Length; i++)
            {
                if (whole[i] == wildcard)
                {
                    runStart = i + 1;
                }
                else if (i - runStart + 1 > len)
                {
                    offset = runStart;
                    len = i - runStart + 1;
                }
            }
            if (len == whole.Length)
            {
                return whole;
            }
            byte[] head = new byte[len];
            Buffer.BlockCopy(whole, offset, head, 0, len);
            return head;
        }

        /// <summary>
        /// 确认整个查找串是否匹配
        /// </summary>
        /// <param name="content">被查找对象</param>
        /// <param name="start">头串匹配位置</param>
        /// <param name="whole">完整查找串</param>
        /// <returns></returns>
        public static bool IsEqual(byte[] content, int start, byte[] whole)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            if (whole == null)
            {
                throw new ArgumentNullException("whole");
            }
            if (start < 0 || start > content.Length - whole.Length)
            {
                return false;
            }
            int i = 0;
            for (i = 0; i < whole.Length; i++)
            {
                if (whole[i] == wildcard)
                {
                    continue;
                }
                if (content[start + i] != whole[i])
                {
                    break;
                }
            }
            if (i == whole.Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
